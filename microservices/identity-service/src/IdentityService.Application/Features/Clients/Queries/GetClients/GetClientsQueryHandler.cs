using IdentityService.Application.Common.Models;
using IdentityService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.Clients.Queries.GetClients;

public class GetClientsQueryHandler : IRequestHandler<GetClientsQuery, Result<GetClientsResponse>>
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<GetClientsQueryHandler> _logger;

    public GetClientsQueryHandler(
        IClientRepository clientRepository,
        ILogger<GetClientsQueryHandler> logger)
    {
        _clientRepository = clientRepository;
        _logger = logger;
    }

    public async Task<Result<GetClientsResponse>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (clients, totalCount) = await _clientRepository.GetClientsPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                request.IsActive,
                cancellationToken);

            var clientDtos = clients.Select(c => new ClientDto(
                c.Id,
                c.ClientId,
                c.Name,
                c.Description,
                c.IsActive,
                c.RequireConsent,
                c.RequirePkce,
                c.AccessTokenLifetime,
                c.RefreshTokenLifetime,
                c.GetAllowedGrantTypes(),
                c.GetAllowedScopes(),
                c.GetRedirectUris(),
                c.LastUsedAt)).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return Result<GetClientsResponse>.Success(new GetClientsResponse(
                clientDtos,
                totalCount,
                request.PageNumber,
                request.PageSize,
                totalPages));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving clients");
            return Result<GetClientsResponse>.Failure("An error occurred while retrieving clients");
        }
    }
}

