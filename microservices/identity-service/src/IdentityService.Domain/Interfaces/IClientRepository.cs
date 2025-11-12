namespace IdentityService.Domain.Interfaces;

/// <summary>
/// Repository interface for OAuth2 client operations
/// </summary>
public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Client?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByClientIdAsync(string clientId, CancellationToken cancellationToken = default);
    Task AddAsync(Client client, CancellationToken cancellationToken = default);
    Task UpdateAsync(Client client, CancellationToken cancellationToken = default);
    Task<List<Client>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(List<Client> Clients, int TotalCount)> GetClientsPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
}

