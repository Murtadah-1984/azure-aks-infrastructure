using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.WebAuthn.Commands.AuthenticateWebAuthn;

public class AuthenticateWebAuthnCommandHandler : IRequestHandler<AuthenticateWebAuthnCommand, Result<AuthenticateWebAuthnResponse>>
{
    private readonly IWebAuthnService _webAuthnService;
    private readonly ILogger<AuthenticateWebAuthnCommandHandler> _logger;

    public AuthenticateWebAuthnCommandHandler(
        IWebAuthnService webAuthnService,
        ILogger<AuthenticateWebAuthnCommandHandler> logger)
    {
        _webAuthnService = webAuthnService;
        _logger = logger;
    }

    public async Task<Result<AuthenticateWebAuthnResponse>> Handle(AuthenticateWebAuthnCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var isValid = await _webAuthnService.CompleteAuthenticationAsync(
                request.UserId,
                request.CredentialId,
                request.AuthenticatorData,
                request.ClientDataJson,
                request.Signature,
                request.Counter,
                cancellationToken);

            if (!isValid)
            {
                return Result<AuthenticateWebAuthnResponse>.Success(new AuthenticateWebAuthnResponse(
                    IsValid: false,
                    Message: "WebAuthn authentication failed"));
            }

            _logger.LogInformation("WebAuthn authentication successful for user: {UserId}", request.UserId);

            return Result<AuthenticateWebAuthnResponse>.Success(new AuthenticateWebAuthnResponse(
                IsValid: true,
                Message: "WebAuthn authentication successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating with WebAuthn");
            return Result<AuthenticateWebAuthnResponse>.Failure("An error occurred while authenticating with WebAuthn");
        }
    }
}

