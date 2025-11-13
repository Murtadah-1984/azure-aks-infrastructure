using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.OAuth2.GrantTypes;

/// <summary>
/// Handler for Authorization Code grant type
/// </summary>
public class AuthorizationCodeGrantHandler : IGrantTypeHandler
{
    public string GrantType => "authorization_code";

    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<AuthorizationCodeGrantHandler> _logger;

    public AuthorizationCodeGrantHandler(
        IAuthorizationCodeRepository authorizationCodeRepository,
        IUserRepository userRepository,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<AuthorizationCodeGrantHandler> logger)
    {
        _authorizationCodeRepository = authorizationCodeRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task<Result<TokenResponse>> HandleAsync(
        TokenRequest request,
        Client client,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Code))
            {
                return Result<TokenResponse>.Failure("Authorization code is required");
            }

            // Get authorization code
            var authCode = await _authorizationCodeRepository.GetByCodeAsync(request.Code, cancellationToken);
            if (authCode == null || !authCode.IsValid())
            {
                return Result<TokenResponse>.Failure("Invalid or expired authorization code");
            }

            // Validate client
            if (authCode.ClientId != client.Id)
            {
                return Result<TokenResponse>.Failure("Authorization code was issued to a different client");
            }

            // Validate redirect URI
            if (!string.IsNullOrEmpty(request.RedirectUri) && authCode.RedirectUri != request.RedirectUri)
            {
                return Result<TokenResponse>.Failure("Redirect URI mismatch");
            }

            // Validate PKCE if required
            if (client.RequirePkce)
            {
                if (string.IsNullOrEmpty(request.CodeVerifier))
                {
                    return Result<TokenResponse>.Failure("Code verifier is required for PKCE");
                }

                if (!authCode.ValidateCodeVerifier(request.CodeVerifier))
                {
                    return Result<TokenResponse>.Failure("Invalid code verifier");
                }
            }

            // Get user
            var user = await _userRepository.GetByIdAsync(authCode.UserId, cancellationToken);
            if (user == null || !user.IsActive)
            {
                return Result<TokenResponse>.Failure("User account is inactive");
            }

            // Mark authorization code as used
            authCode.MarkAsUsed();
            await _authorizationCodeRepository.UpdateAsync(authCode, cancellationToken);

            // Get user roles
            var roles = user.UserRoles.Select(ur => ur.RoleName).ToList();

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = RefreshToken.Create(
                user.Id,
                Guid.NewGuid().ToString(),
                DateTime.UtcNow.AddDays(30),
                null,
                null);

            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

            _logger.LogInformation("Authorization code exchanged for tokens for user: {UserId}", user.Id);

            return Result<TokenResponse>.Success(new TokenResponse
            {
                AccessToken = accessToken,
                TokenType = "Bearer",
                ExpiresIn = 3600,
                RefreshToken = refreshToken.Token,
                Scope = authCode.Scope
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling authorization code grant");
            return Result<TokenResponse>.Failure("An error occurred while processing the authorization code");
        }
    }
}

