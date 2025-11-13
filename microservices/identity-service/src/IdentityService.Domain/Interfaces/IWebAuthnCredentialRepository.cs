using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces;

/// <summary>
/// Repository interface for WebAuthn credential operations
/// </summary>
public interface IWebAuthnCredentialRepository
{
    Task<WebAuthnCredential?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WebAuthnCredential?> GetByCredentialIdAsync(string credentialId, CancellationToken cancellationToken = default);
    Task<List<WebAuthnCredential>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(WebAuthnCredential credential, CancellationToken cancellationToken = default);
    Task UpdateAsync(WebAuthnCredential credential, CancellationToken cancellationToken = default);
    Task DeleteAsync(WebAuthnCredential credential, CancellationToken cancellationToken = default);
}

