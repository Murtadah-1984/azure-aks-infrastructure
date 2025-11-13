using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.OAuth2.GrantTypes;

/// <summary>
/// Handler for Client Credentials grant type (service-to-service)
/// </summary>
public class ClientCredentialsGrantHandler : IGrantTypeHandler
{
    public string GrantType => "client_credentials";

    private readonly ITokenService _tokenService;
    private readonly ILogger<ClientCredentialsGrantHandler> _logger;

    public ClientCredentialsGrantHandler(
        ITokenService tokenService,
        ILogger<ClientCredentialsGrantHandler> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Result<TokenResponse>> HandleAsync(
        TokenRequest request,
        Client client,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Client credentials flow doesn't require a user
            // Generate a token with client claims
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("client_id", client.ClientId),
                new System.Security.Claims.Claim("client_name", client.Name)
            };

            // Generate access token (simplified - in production, use proper token generation)
            var accessToken = _tokenService.GenerateAccessTokenForClient(client, request.Scope);

            _logger.LogInformation("Client credentials token issued for client: {ClientId}", client.ClientId);

            return Result<TokenResponse>.Success(new TokenResponse
            {
                AccessToken = accessToken,
                TokenType = "Bearer",
                ExpiresIn = client.AccessTokenLifetime,
                Scope = request.Scope
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling client credentials grant");
            return Result<TokenResponse>.Failure("An error occurred while processing client credentials");
        }
    }
}

