using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for signing key operations
/// </summary>
public class SigningKeyRepository : ISigningKeyRepository
{
    private readonly ApplicationDbContext _context;

    public SigningKeyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SigningKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<SigningKey>()
            .FirstOrDefaultAsync(k => k.Id == id, cancellationToken);
    }

    public async Task<SigningKey?> GetByKeyIdAsync(string keyId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<SigningKey>()
            .FirstOrDefaultAsync(k => k.KeyId == keyId, cancellationToken);
    }

    public async Task<List<SigningKey>> GetActiveKeysAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<SigningKey>()
            .Where(k => k.IsActive && (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow))
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SigningKey>> GetAllKeysAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<SigningKey>()
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SigningKey key, CancellationToken cancellationToken = default)
    {
        await _context.Set<SigningKey>().AddAsync(key, cancellationToken);
    }

    public async Task UpdateAsync(SigningKey key, CancellationToken cancellationToken = default)
    {
        _context.Set<SigningKey>().Update(key);
        await Task.CompletedTask;
    }
}

