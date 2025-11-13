using IdentityService.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Security.Cryptography;

namespace IdentityService.Infrastructure.Services.MfaProviders;

/// <summary>
/// Email MFA provider implementation
/// </summary>
public class EmailMfaProvider : IMfaProvider
{
    public string ProviderType => "Email";

    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailMfaProvider> _logger;
    private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _cache;

    public EmailMfaProvider(
        IConfiguration configuration,
        ILogger<EmailMfaProvider> logger,
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
            // Store code in Redis with 10-minute TTL
            var cacheKey = $"mfa:email:{userId}:{identifier}";
            var options = new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };
            await _cache.SetStringAsync(cacheKey, code, options, cancellationToken);

            // Send email via SMTP
            var smtpHost = _configuration["Mfa:Email:SmtpHost"] ?? "localhost";
            var smtpPort = int.Parse(_configuration["Mfa:Email:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["Mfa:Email:SmtpUsername"] ?? "";
            var smtpPassword = _configuration["Mfa:Email:SmtpPassword"] ?? "";
            var fromEmail = _configuration["Mfa:Email:FromEmail"] ?? "noreply@identityservice.com";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = string.IsNullOrEmpty(smtpUsername) ? null : new System.Net.NetworkCredential(smtpUsername, smtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = "Your Multi-Factor Authentication Code",
                Body = $"Your MFA code is: {code}\n\nThis code will expire in 10 minutes.\n\nIf you didn't request this code, please ignore this email.",
                IsBodyHtml = false
            };

            mailMessage.To.Add(identifier);

            await client.SendMailAsync(mailMessage, cancellationToken);

            _logger.LogInformation("Email MFA code sent to {Email} for user {UserId}", identifier, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email MFA code");
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
            var cacheKey = $"mfa:email:{userId}:{identifier}";
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
            _logger.LogError(ex, "Error verifying email MFA code");
            return false;
        }
    }
}

