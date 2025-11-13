using IdentityService.Application.Common.Models;
using MediatR;

namespace IdentityService.Application.Features.Clients.Queries.GetClients;

public record GetClientsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null) : IRequest<Result<GetClientsResponse>>;

public record GetClientsResponse(
    List<ClientDto> Clients,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

public record ClientDto(
    Guid Id,
    string ClientId,
    string Name,
    string? Description,
    bool IsActive,
    bool RequireConsent,
    bool RequirePkce,
    int AccessTokenLifetime,
    int RefreshTokenLifetime,
    List<string> AllowedGrantTypes,
    List<string> AllowedScopes,
    List<string> RedirectUris,
    DateTime? LastUsedAt);

