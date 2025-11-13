using IdentityService.Application.Common.Models;
using MediatR;

namespace IdentityService.Application.Features.Clients.Commands.CreateClient;

public record CreateClientCommand(
    string ClientId,
    string ClientSecret,
    string Name,
    string? Description = null,
    bool RequireConsent = true,
    bool RequirePkce = false,
    int AccessTokenLifetime = 3600,
    int RefreshTokenLifetime = 30,
    List<string>? AllowedGrantTypes = null,
    List<string>? AllowedScopes = null,
    List<string>? RedirectUris = null,
    List<string>? PostLogoutRedirectUris = null) : IRequest<Result<CreateClientResponse>>;

public record CreateClientResponse(
    Guid ClientId,
    string ClientIdentifier,
    string Name);

