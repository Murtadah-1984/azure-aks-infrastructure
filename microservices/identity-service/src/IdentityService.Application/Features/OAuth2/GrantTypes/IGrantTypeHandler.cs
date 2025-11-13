using IdentityService.Application.Common.Models;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Features.OAuth2.GrantTypes;

/// <summary>
/// Interface for OAuth2 grant type handlers (Strategy Pattern)
/// </summary>
public interface IGrantTypeHandler
{
    string GrantType { get; } // "authorization_code", "client_credentials", "refresh_token", "device_code"
    
    Task<Result<TokenResponse>> HandleAsync(
        TokenRequest request,
        Client client,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Token request model
/// </summary>
public class TokenRequest
{
    public string GrantType { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? RedirectUri { get; set; }
    public string? CodeVerifier { get; set; }
    public string? RefreshToken { get; set; }
    public string? DeviceCode { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Scope { get; set; }
}

/// <summary>
/// Token response model
/// </summary>
public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public string? RefreshToken { get; set; }
    public string? Scope { get; set; }
    public string? IdToken { get; set; } // For OIDC
}

