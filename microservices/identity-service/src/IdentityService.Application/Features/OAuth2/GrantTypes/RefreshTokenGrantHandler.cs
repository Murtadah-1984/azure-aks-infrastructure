using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.OAuth2.GrantTypes;

/// <summary>
/// Handler for Refresh Token grant type
/// </summary>
public class RefreshTokenGrantHandler : IGrantTypeHandler
{
    public string GrantType => "refresh_token";

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RefreshTokenGrantHandler> _logger;

    public RefreshTokenGrantHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        ITokenService tokenService,
        ILogger<RefreshTokenGrantHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
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
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return Result<TokenResponse>.Failure("Refresh token is required");
            }

            // Get refresh token
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
            if (refreshToken == null || !refreshToken.IsValid())
            {
                return Result<TokenResponse>.Failure("Invalid or expired refresh token");
            }

            // Validate client if refresh token has client association
            // (In a full implementation, refresh tokens might be associated with clients)

            // Get user
            var user = await _userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
            if (user == null || !user.IsActive)
            {
                return Result<TokenResponse>.Failure("User account is inactive");
            }

            // Revoke old refresh token (token rotation)
            refreshToken.Revoke();
            await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);

            // Get user roles
            var roles = user.UserRoles.Select(ur => ur.RoleName).ToList();

            // Generate new tokens
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = RefreshToken.Create(
                user.Id,
                _tokenService.GenerateRefreshToken(),
                _tokenService.GetRefreshTokenExpiration(),
                null,
                null);

            await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

            _logger.LogInformation("Refresh token exchanged for new tokens for user: {UserId}", user.Id);

            return Result<TokenResponse>.Success(new TokenResponse
            {
                AccessToken = accessToken,
                TokenType = "Bearer",
                ExpiresIn = client.AccessTokenLifetime,
                RefreshToken = newRefreshToken.Token,
                Scope = request.Scope
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling refresh token grant");
            return Result<TokenResponse>.Failure("An error occurred while processing the refresh token");
        }
    }
}

