using IdentityService.Application.Common.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace IdentityService.Infrastructure.Services;

/// <summary>
/// WebAuthn/FIDO2 service implementation
/// Note: This is a simplified implementation. For production, use Fido2NetLib package
/// </summary>
public class WebAuthnService : IWebAuthnService
{
    private readonly IWebAuthnCredentialRepository _credentialRepository;
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebAuthnService> _logger;

    public WebAuthnService(
        IWebAuthnCredentialRepository credentialRepository,
        IDistributedCache cache,
        IConfiguration configuration,
        ILogger<WebAuthnService> logger)
    {
        _credentialRepository = credentialRepository;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<object> CreateRegistrationChallengeAsync(
        Guid userId,
        string userName,
        string userDisplayName,
        CancellationToken cancellationToken = default)
    {
        // Generate challenge
        var challengeBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(challengeBytes);
        var challenge = Convert.ToBase64UrlString(challengeBytes);

        // Store challenge in Redis with 5-minute TTL
        var cacheKey = $"webauthn:register:{userId}";
        var challengeData = new
        {
            Challenge = challenge,
            UserId = userId,
            UserName = userName,
            UserDisplayName = userDisplayName,
            CreatedAt = DateTime.UtcNow
        };
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(challengeData), options, cancellationToken);

        // Return public key credential creation options
        var rpId = _configuration["WebAuthn:RpId"] ?? new Uri(_configuration["WebAuthn:Origin"] ?? "https://localhost").Host;
        var origin = _configuration["WebAuthn:Origin"] ?? "https://localhost";

        return new
        {
            challenge = challenge,
            rp = new
            {
                name = _configuration["WebAuthn:RpName"] ?? "Identity Service",
                id = rpId
            },
            user = new
            {
                id = Convert.ToBase64UrlString(userId.ToByteArray()),
                name = userName,
                displayName = userDisplayName
            },
            pubKeyCredParams = new[]
            {
                new { type = "public-key", alg = -7 }, // ES256
                new { type = "public-key", alg = -257 } // RS256
            },
            timeout = 60000,
            attestation = "direct"
        };
    }

    public async Task<Guid> CompleteRegistrationAsync(
        Guid userId,
        string credentialId,
        string publicKey,
        int counter,
        byte[] attestationObject,
        byte[] clientDataJson,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify challenge
            var cacheKey = $"webauthn:register:{userId}";
            var challengeDataJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (string.IsNullOrEmpty(challengeDataJson))
            {
                throw new InvalidOperationException("Registration challenge not found or expired");
            }

            // In production, verify attestation object and client data JSON
            // For now, we'll create the credential

            // Create WebAuthn credential
            var credential = WebAuthnCredential.Create(
                userId,
                credentialId,
                publicKey,
                counter);

            await _credentialRepository.AddAsync(credential, cancellationToken);

            // Remove challenge
            await _cache.RemoveAsync(cacheKey, cancellationToken);

            _logger.LogInformation("WebAuthn credential registered for user: {UserId}", userId);

            return credential.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing WebAuthn registration");
            throw;
        }
    }

    public async Task<object> CreateAuthenticationChallengeAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Get user's credentials
        var credentials = await _credentialRepository.GetByUserIdAsync(userId, cancellationToken);
        var activeCredentials = credentials.Where(c => c.IsActive).ToList();

        if (!activeCredentials.Any())
        {
            throw new InvalidOperationException("No active WebAuthn credentials found for user");
        }

        // Generate challenge
        var challengeBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(challengeBytes);
        var challenge = Convert.ToBase64UrlString(challengeBytes);

        // Store challenge in Redis with 5-minute TTL
        var cacheKey = $"webauthn:auth:{userId}";
        var challengeData = new
        {
            Challenge = challenge,
            UserId = userId,
            AllowedCredentialIds = activeCredentials.Select(c => c.CredentialId).ToList(),
            CreatedAt = DateTime.UtcNow
        };
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(challengeData), options, cancellationToken);

        // Return public key credential request options
        var rpId = _configuration["WebAuthn:RpId"] ?? new Uri(_configuration["WebAuthn:Origin"] ?? "https://localhost").Host;

        return new
        {
            challenge = challenge,
            rpId = rpId,
            allowCredentials = activeCredentials.Select(c => new
            {
                type = "public-key",
                id = c.CredentialId,
                transports = new[] { "usb", "nfc", "ble", "internal" }
            }).ToArray(),
            timeout = 60000,
            userVerification = "preferred"
        };
    }

    public async Task<bool> CompleteAuthenticationAsync(
        Guid userId,
        string credentialId,
        byte[] authenticatorData,
        byte[] clientDataJson,
        byte[] signature,
        int counter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify challenge
            var cacheKey = $"webauthn:auth:{userId}";
            var challengeDataJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (string.IsNullOrEmpty(challengeDataJson))
            {
                throw new InvalidOperationException("Authentication challenge not found or expired");
            }

            // Get credential
            var credential = await _credentialRepository.GetByCredentialIdAsync(credentialId, cancellationToken);
            if (credential == null || !credential.IsActive || credential.UserId != userId)
            {
                return false;
            }

            // Verify counter (must be greater than stored counter)
            if (counter <= credential.Counter)
            {
                _logger.LogWarning("WebAuthn counter replay detected for credential: {CredentialId}", credentialId);
                return false;
            }

            // In production, verify signature using public key
            // For now, we'll update the counter

            // Update credential counter
            credential.UpdateCounter(counter);
            await _credentialRepository.UpdateAsync(credential, cancellationToken);

            // Remove challenge
            await _cache.RemoveAsync(cacheKey, cancellationToken);

            _logger.LogInformation("WebAuthn authentication successful for user: {UserId}", userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing WebAuthn authentication");
            return false;
        }
    }
}

