using IdentityService.Domain.Entities;
using System.Security.Claims;

namespace IdentityService.Application.Common.Interfaces;

/// <summary>
/// Service for generating and validating JWT tokens
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates an access token for a user
    /// </summary>
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    
    /// <summary>
    /// Generates a refresh token
    /// </summary>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Validates a token and returns the principal
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);
    
    /// <summary>
    /// Gets the expiration time for access tokens
    /// </summary>
    DateTime GetAccessTokenExpiration();
    
    /// <summary>
    /// Gets the expiration time for refresh tokens
    /// </summary>
    DateTime GetRefreshTokenExpiration();
    
    /// <summary>
    /// Generates an access token for a client (client credentials flow)
    /// </summary>
    string GenerateAccessTokenForClient(Domain.Entities.Client client, string? scope = null);
}

