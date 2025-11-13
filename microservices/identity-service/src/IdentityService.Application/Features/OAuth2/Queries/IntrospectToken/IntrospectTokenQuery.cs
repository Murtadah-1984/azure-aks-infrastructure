using IdentityService.Application.Common.Models;
using MediatR;

namespace IdentityService.Application.Features.OAuth2.Queries.IntrospectToken;

public record IntrospectTokenQuery(
    string Token,
    string? TokenTypeHint = null) : IRequest<Result<IntrospectTokenResponse>>;

public record IntrospectTokenResponse(
    bool Active,
    string? Scope = null,
    string? ClientId = null,
    string? Username = null,
    long? Exp = null,
    long? Iat = null,
    string? Sub = null,
    string? Aud = null,
    string? Iss = null,
    string? Jti = null);

