namespace IdentityService.Application.Common.Interfaces;

/// <summary>
/// Interface for MFA providers (Strategy Pattern)
/// </summary>
public interface IMfaProvider
{
    string ProviderType { get; } // "TOTP", "SMS", "Email"
    
    Task<string> GenerateCodeAsync(
        Guid userId,
        string identifier, // Phone number or email
        CancellationToken cancellationToken = default);
    
    Task<bool> SendCodeAsync(
        Guid userId,
        string identifier,
        string code,
        CancellationToken cancellationToken = default);
    
    Task<bool> VerifyCodeAsync(
        Guid userId,
        string identifier,
        string code,
        CancellationToken cancellationToken = default);
}

