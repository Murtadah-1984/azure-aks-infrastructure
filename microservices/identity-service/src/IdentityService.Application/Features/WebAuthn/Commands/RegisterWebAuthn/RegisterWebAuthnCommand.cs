using IdentityService.Application.Common.Models;
using MediatR;

namespace IdentityService.Application.Features.WebAuthn.Commands.RegisterWebAuthn;

public record RegisterWebAuthnCommand(
    Guid UserId,
    string CredentialId,
    string PublicKey,
    int Counter,
    byte[] AttestationObject,
    byte[] ClientDataJson,
    string? Name = null) : IRequest<Result<RegisterWebAuthnResponse>>;

public record RegisterWebAuthnResponse(
    Guid CredentialId,
    string CredentialIdentifier);

