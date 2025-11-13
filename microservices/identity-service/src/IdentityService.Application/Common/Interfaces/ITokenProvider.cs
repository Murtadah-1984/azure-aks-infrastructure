using IdentityService.Domain.Entities;

namespace IdentityService.Application.Common.Interfaces;

/// <summary>
/// Interface for token providers (Strategy Pattern)
/// </summary>
public interface ITokenProvider
{
    string ProviderType { get; } // "JWT" or "Reference"
    
    Task<string> GenerateAccessTokenAsync(
        User user,
        List<string> roles,
        List<string> permissions,
        CancellationToken cancellationToken = default);
    
    Task<string> GenerateRefreshTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    
    Task<bool> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default);
    
    Task<Dictionary<string, object>> GetTokenClaimsAsync(
        string token,
        CancellationToken cancellationToken = default);
}

