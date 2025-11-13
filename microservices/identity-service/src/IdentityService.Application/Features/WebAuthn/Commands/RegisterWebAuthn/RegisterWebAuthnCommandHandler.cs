using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.WebAuthn.Commands.RegisterWebAuthn;

public class RegisterWebAuthnCommandHandler : IRequestHandler<RegisterWebAuthnCommand, Result<RegisterWebAuthnResponse>>
{
    private readonly IWebAuthnService _webAuthnService;
    private readonly ILogger<RegisterWebAuthnCommandHandler> _logger;

    public RegisterWebAuthnCommandHandler(
        IWebAuthnService webAuthnService,
        ILogger<RegisterWebAuthnCommandHandler> logger)
    {
        _webAuthnService = webAuthnService;
        _logger = logger;
    }

    public async Task<Result<RegisterWebAuthnResponse>> Handle(RegisterWebAuthnCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var credentialId = await _webAuthnService.CompleteRegistrationAsync(
                request.UserId,
                request.CredentialId,
                request.PublicKey,
                request.Counter,
                request.AttestationObject,
                request.ClientDataJson,
                cancellationToken);

            _logger.LogInformation("WebAuthn credential registered for user: {UserId}", request.UserId);

            return Result<RegisterWebAuthnResponse>.Success(new RegisterWebAuthnResponse(
                credentialId,
                request.CredentialId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering WebAuthn credential");
            return Result<RegisterWebAuthnResponse>.Failure("An error occurred while registering the WebAuthn credential");
        }
    }
}

