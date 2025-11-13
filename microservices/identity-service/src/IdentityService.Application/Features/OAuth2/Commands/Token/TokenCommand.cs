using IdentityService.Application.Common.Models;
using MediatR;

namespace IdentityService.Application.Features.OAuth2.Commands.Token;

public record TokenCommand(
    string GrantType,
    string? Code = null,
    string? RedirectUri = null,
    string? ClientId = null,
    string? ClientSecret = null,
    string? CodeVerifier = null,
    string? RefreshToken = null,
    string? Scope = null) : IRequest<Result<TokenResponse>>;

public record TokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string? RefreshToken = null,
    string? IdToken = null,
    string? Scope = null);

