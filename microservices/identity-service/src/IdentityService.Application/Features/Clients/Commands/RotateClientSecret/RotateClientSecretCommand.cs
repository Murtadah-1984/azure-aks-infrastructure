using IdentityService.Application.Common.Models;
using MediatR;

namespace IdentityService.Application.Features.Clients.Commands.RotateClientSecret;

public record RotateClientSecretCommand(
    Guid ClientId,
    string NewClientSecret) : IRequest<Result<RotateClientSecretResponse>>;

public record RotateClientSecretResponse(
    Guid ClientId,
    string ClientIdentifier);

