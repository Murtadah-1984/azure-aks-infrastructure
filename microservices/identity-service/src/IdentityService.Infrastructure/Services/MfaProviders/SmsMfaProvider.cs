using IdentityService.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace IdentityService.Infrastructure.Services.MfaProviders;

/// <summary>
/// SMS MFA provider implementation (using Twilio or AWS SNS)
/// </summary>
public class SmsMfaProvider : IMfaProvider
{
    public string ProviderType => "SMS";

    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsMfaProvider> _logger;
    private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _cache;

    public SmsMfaProvider(
        IConfiguration configuration,
        ILogger<SmsMfaProvider> logger,
        Microsoft.Extensions.Caching.Distributed.IDistributedCache cache)
    {
        _configuration = configuration;
        _logger = logger;
        _cache = cache;
    }

    public Task<string> GenerateCodeAsync(
        Guid userId,
        string identifier,
        CancellationToken cancellationToken = default)
    {
        // Generate 6-digit code
        var randomBytes = new byte[4];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var code = (BitConverter.ToUInt32(randomBytes, 0) % 900000 + 100000).ToString();

        return Task.FromResult(code);
    }

    public async Task<bool> SendCodeAsync(
        Guid userId,
        string identifier,
        string code,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Store code in Redis with 5-minute TTL
            var cacheKey = $"mfa:sms:{userId}:{identifier}";
            var options = new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, code, options, cancellationToken);

            // In production, send SMS via Twilio or AWS SNS
            var provider = _configuration["Mfa:Sms:Provider"] ?? "Twilio";
            
            if (provider == "Twilio")
            {
                // TODO: Implement Twilio integration
                _logger.LogInformation("SMS MFA code sent to {PhoneNumber} for user {UserId}", identifier, userId);
            }
            else if (provider == "AWSSNS")
            {
                // TODO: Implement AWS SNS integration
                _logger.LogInformation("SMS MFA code sent via AWS SNS to {PhoneNumber} for user {UserId}", identifier, userId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS MFA code");
            return false;
        }
    }

    public async Task<bool> VerifyCodeAsync(
        Guid userId,
        string identifier,
        string code,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"mfa:sms:{userId}:{identifier}";
            var storedCode = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (string.IsNullOrEmpty(storedCode))
            {
                return false;
            }

            var isValid = storedCode == code;

            if (isValid)
            {
                // Remove code after successful verification
                await _cache.RemoveAsync(cacheKey, cancellationToken);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying SMS MFA code");
            return false;
        }
    }
}

