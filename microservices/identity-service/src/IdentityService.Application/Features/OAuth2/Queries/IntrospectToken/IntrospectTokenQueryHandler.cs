using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

namespace IdentityService.Application.Features.OAuth2.Queries.IntrospectToken;

public class IntrospectTokenQueryHandler : IRequestHandler<IntrospectTokenQuery, Result<IntrospectTokenResponse>>
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<IntrospectTokenQueryHandler> _logger;

    public IntrospectTokenQueryHandler(
        ITokenService tokenService,
        ILogger<IntrospectTokenQueryHandler> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    public Task<Result<IntrospectTokenResponse>> Handle(IntrospectTokenQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate token
            var principal = _tokenService.ValidateToken(request.Token);
            
            if (principal == null)
            {
                // Token is invalid or expired
                return Task.FromResult(Result<IntrospectTokenResponse>.Success(new IntrospectTokenResponse(
                    Active: false)));
            }

            // Extract claims from token
            var claims = principal.Claims.ToDictionary(c => c.Type, c => c.Value);
            
            // Get expiration
            long? exp = null;
            if (claims.TryGetValue("exp", out var expStr))
            {
                if (long.TryParse(expStr, out var expValue))
                {
                    exp = expValue;
                }
            }

            // Get issued at
            long? iat = null;
            if (claims.TryGetValue("iat", out var iatStr))
            {
                if (long.TryParse(iatStr, out var iatValue))
                {
                    iat = iatValue;
                }
            }

            // Check if token is still active (not expired)
            var isActive = exp == null || DateTimeOffset.FromUnixTimeSeconds(exp.Value) > DateTimeOffset.UtcNow;

            var response = new IntrospectTokenResponse(
                Active: isActive,
                Scope: claims.GetValueOrDefault("scope"),
                ClientId: claims.GetValueOrDefault("client_id"),
                Username: claims.GetValueOrDefault(System.Security.Claims.ClaimTypes.Name),
                Exp: exp,
                Iat: iat,
                Sub: claims.GetValueOrDefault("sub"),
                Aud: claims.GetValueOrDefault("aud"),
                Iss: claims.GetValueOrDefault("iss"),
                Jti: claims.GetValueOrDefault("jti"));

            return Task.FromResult(Result<IntrospectTokenResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error introspecting token");
            return Task.FromResult(Result<IntrospectTokenResponse>.Success(new IntrospectTokenResponse(
                Active: false)));
        }
    }
}

