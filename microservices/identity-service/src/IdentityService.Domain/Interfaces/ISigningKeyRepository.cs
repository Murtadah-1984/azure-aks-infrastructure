using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces;

/// <summary>
/// Repository interface for signing key operations
/// </summary>
public interface ISigningKeyRepository
{
    Task<SigningKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SigningKey?> GetByKeyIdAsync(string keyId, CancellationToken cancellationToken = default);
    Task<List<SigningKey>> GetActiveKeysAsync(CancellationToken cancellationToken = default);
    Task<List<SigningKey>> GetAllKeysAsync(CancellationToken cancellationToken = default);
    Task AddAsync(SigningKey key, CancellationToken cancellationToken = default);
    Task UpdateAsync(SigningKey key, CancellationToken cancellationToken = default);
}

