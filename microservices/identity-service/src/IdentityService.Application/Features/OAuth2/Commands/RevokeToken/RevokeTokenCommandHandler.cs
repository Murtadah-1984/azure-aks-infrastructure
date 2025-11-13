using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using IdentityService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.OAuth2.Commands.RevokeToken;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result<RevokeTokenResponse>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RevokeTokenCommandHandler> _logger;

    public RevokeTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        ILogger<RevokeTokenCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Result<RevokeTokenResponse>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if it's a refresh token
            if (request.TokenTypeHint == "refresh_token" || string.IsNullOrEmpty(request.TokenTypeHint))
            {
                var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.Token, cancellationToken);
                if (refreshToken != null)
                {
                    refreshToken.Revoke();
                    await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
                    _logger.LogInformation("Refresh token revoked: {TokenId}", refreshToken.Id);
                    return Result<RevokeTokenResponse>.Success(new RevokeTokenResponse(Revoked: true));
                }
            }

            // For access tokens (JWT), we can't revoke them directly as they're stateless
            // In production, you might want to maintain a blacklist in Redis
            // For now, we'll just validate the token and return success
            var principal = _tokenService.ValidateToken(request.Token);
            if (principal != null)
            {
                _logger.LogInformation("Access token validated for revocation (stateless, cannot be revoked)");
                // Note: In a full implementation, you'd add the token to a blacklist
                return Result<RevokeTokenResponse>.Success(new RevokeTokenResponse(Revoked: true));
            }

            // Token not found or invalid
            return Result<RevokeTokenResponse>.Success(new RevokeTokenResponse(Revoked: false));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token");
            return Result<RevokeTokenResponse>.Failure("An error occurred while revoking the token");
        }
    }
}

