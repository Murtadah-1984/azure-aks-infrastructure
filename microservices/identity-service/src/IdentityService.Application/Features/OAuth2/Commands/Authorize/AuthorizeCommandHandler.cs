using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace IdentityService.Application.Features.OAuth2.Commands.Authorize;

public class AuthorizeCommandHandler : IRequestHandler<AuthorizeCommand, Result<AuthorizeResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IClientRepository _clientRepository;
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly IConsentRepository _consentRepository;
    private readonly ILogger<AuthorizeCommandHandler> _logger;

    public AuthorizeCommandHandler(
        ICurrentUserService currentUserService,
        IClientRepository clientRepository,
        IAuthorizationCodeRepository authorizationCodeRepository,
        IConsentRepository consentRepository,
        ILogger<AuthorizeCommandHandler> logger)
    {
        _currentUserService = currentUserService;
        _clientRepository = clientRepository;
        _authorizationCodeRepository = authorizationCodeRepository;
        _consentRepository = consentRepository;
        _logger = logger;
    }

    public async Task<Result<AuthorizeResponse>> Handle(AuthorizeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
            {
                return Result<AuthorizeResponse>.Failure("User must be authenticated");
            }

            if (request.ResponseType != "code")
            {
                return Result<AuthorizeResponse>.Failure("Only authorization code flow is supported");
            }

            // Validate client
            var client = await _clientRepository.GetByClientIdAsync(request.ClientId, cancellationToken);
            if (client == null || !client.IsActive)
            {
                return Result<AuthorizeResponse>.Failure("Invalid or inactive client");
            }

            // Validate redirect URI
            if (!client.IsRedirectUriAllowed(request.RedirectUri))
            {
                return Result<AuthorizeResponse>.Failure("Invalid redirect URI");
            }

            // Check if grant type is allowed
            if (!client.IsGrantTypeAllowed("authorization_code"))
            {
                return Result<AuthorizeResponse>.Failure("Authorization code grant type is not allowed for this client");
            }

            // Check PKCE requirement
            if (client.RequirePkce)
            {
                if (string.IsNullOrEmpty(request.CodeChallenge) || string.IsNullOrEmpty(request.CodeChallengeMethod))
                {
                    return Result<AuthorizeResponse>.Failure("PKCE is required for this client. Code challenge and method are required.");
                }

                if (request.CodeChallengeMethod != "S256" && request.CodeChallengeMethod != "plain")
                {
                    return Result<AuthorizeResponse>.Failure("Invalid code challenge method. Must be 'S256' or 'plain'");
                }
            }

            // Check consent if required
            var userId = Guid.Parse(_currentUserService.UserId);
            if (client.RequireConsent)
            {
                var consent = await _consentRepository.GetByUserAndClientAsync(userId, client.Id, cancellationToken);
                if (consent == null || !consent.IsValid())
                {
                    // Consent required but not granted
                    return Result<AuthorizeResponse>.Failure("CONSENT_REQUIRED");
                }
            }

            // Generate authorization code
            var codeBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(codeBytes);
            var code = Convert.ToBase64String(codeBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "")
                .Substring(0, 32);

            // Create authorization code entity
            var authorizationCode = new AuthorizationCode(
                code,
                client.Id,
                userId,
                request.RedirectUri,
                DateTime.UtcNow.AddMinutes(10), // 10 minute expiration
                request.CodeChallenge,
                request.CodeChallengeMethod,
                request.Scope,
                request.State);

            await _authorizationCodeRepository.AddAsync(authorizationCode, cancellationToken);

            _logger.LogInformation("Authorization code generated for client: {ClientId}, user: {UserId}", request.ClientId, userId);

            return Result<AuthorizeResponse>.Success(new AuthorizeResponse(
                code,
                request.State));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating authorization code");
            return Result<AuthorizeResponse>.Failure("An error occurred while generating authorization code");
        }
    }
}

