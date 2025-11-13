using IdentityService.Application.Common.Models;
using MediatR;

namespace IdentityService.Application.Features.WebAuthn.Commands.AuthenticateWebAuthn;

public record AuthenticateWebAuthnCommand(
    Guid UserId,
    string CredentialId,
    byte[] AuthenticatorData,
    byte[] ClientDataJson,
    byte[] Signature,
    int Counter) : IRequest<Result<AuthenticateWebAuthnResponse>>;

public record AuthenticateWebAuthnResponse(
    bool IsValid,
    string? Message = null);

