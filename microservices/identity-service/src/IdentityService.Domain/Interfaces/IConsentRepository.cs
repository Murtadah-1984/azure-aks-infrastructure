using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces;

/// <summary>
/// Repository interface for user consent operations
/// </summary>
public interface IConsentRepository
{
    Task<Consent?> GetByUserAndClientAsync(Guid userId, Guid clientId, CancellationToken cancellationToken = default);
    Task<List<Consent>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Consent>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task AddAsync(Consent consent, CancellationToken cancellationToken = default);
    Task UpdateAsync(Consent consent, CancellationToken cancellationToken = default);
    Task DeleteExpiredConsentsAsync(CancellationToken cancellationToken = default);
}

