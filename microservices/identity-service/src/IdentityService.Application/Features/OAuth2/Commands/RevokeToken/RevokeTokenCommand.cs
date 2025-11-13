using IdentityService.Application.Common.Models;
using MediatR;

namespace IdentityService.Application.Features.OAuth2.Commands.RevokeToken;

public record RevokeTokenCommand(
    string Token,
    string? TokenTypeHint = null) : IRequest<Result<RevokeTokenResponse>>;

public record RevokeTokenResponse(bool Revoked);

