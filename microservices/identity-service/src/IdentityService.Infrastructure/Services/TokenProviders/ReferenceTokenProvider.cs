using IdentityService.Application.Common.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace IdentityService.Infrastructure.Services.TokenProviders;

/// <summary>
/// Reference token provider implementation (opaque tokens stored in Redis)
/// </summary>
public class ReferenceTokenProvider : ITokenProvider
{
    public string ProviderType => "Reference";

    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReferenceTokenProvider> _logger;
    private readonly int _accessTokenExpirationMinutes;

    public ReferenceTokenProvider(
        IDistributedCache cache,
        IConfiguration configuration,
        ILogger<ReferenceTokenProvider> logger)
    {
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
        _accessTokenExpirationMinutes = int.Parse(configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");
    }

    public async Task<string> GenerateAccessTokenAsync(
        User user,
        List<string> roles,
        List<string> permissions,
        CancellationToken cancellationToken = default)
    {
        // Generate opaque reference token
        var randomBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var token = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        // Store token data in Redis
        var tokenData = new
        {
            UserId = user.Id.ToString(),
            Email = user.Email,
            Username = user.Username,
            Roles = roles,
            Permissions = permissions,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes)
        };

        var tokenDataJson = JsonSerializer.Serialize(tokenData);
        var cacheKey = $"ref_token:{token}";
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_accessTokenExpirationMinutes)
        };

        await _cache.SetStringAsync(cacheKey, tokenDataJson, options, cancellationToken);
        return token;
    }

    public Task<string> GenerateRefreshTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Refresh tokens are opaque strings
        var randomBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var refreshToken = Convert.ToBase64String(randomBytes);
        return Task.FromResult(refreshToken);
    }

    public async Task<bool> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"ref_token:{token}";
            var tokenDataJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
            
            if (string.IsNullOrEmpty(tokenDataJson))
                return false;

            var tokenData = JsonSerializer.Deserialize<TokenData>(tokenDataJson);
            if (tokenData == null)
                return false;

            // Check expiration
            if (tokenData.ExpiresAt < DateTime.UtcNow)
            {
                await _cache.RemoveAsync(cacheKey, cancellationToken);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Reference token validation failed");
            return false;
        }
    }

    public async Task<Dictionary<string, object>> GetTokenClaimsAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"ref_token:{token}";
            var tokenDataJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
            
            if (string.IsNullOrEmpty(tokenDataJson))
                return new Dictionary<string, object>();

            var tokenData = JsonSerializer.Deserialize<TokenData>(tokenDataJson);
            if (tokenData == null)
                return new Dictionary<string, object>();

            return new Dictionary<string, object>
            {
                { "sub", tokenData.UserId },
                { "email", tokenData.Email },
                { "name", tokenData.Username },
                { "roles", tokenData.Roles },
                { "permissions", tokenData.Permissions }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract reference token claims");
            return new Dictionary<string, object>();
        }
    }

    private class TokenData
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public DateTime ExpiresAt { get; set; }
    }
}

