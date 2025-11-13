using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for WebAuthn credential operations
/// </summary>
public class WebAuthnCredentialRepository : IWebAuthnCredentialRepository
{
    private readonly ApplicationDbContext _context;

    public WebAuthnCredentialRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WebAuthnCredential?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<WebAuthnCredential>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<WebAuthnCredential?> GetByCredentialIdAsync(string credentialId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<WebAuthnCredential>()
            .FirstOrDefaultAsync(c => c.CredentialId == credentialId, cancellationToken);
    }

    public async Task<List<WebAuthnCredential>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<WebAuthnCredential>()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WebAuthnCredential credential, CancellationToken cancellationToken = default)
    {
        await _context.Set<WebAuthnCredential>().AddAsync(credential, cancellationToken);
    }

    public async Task UpdateAsync(WebAuthnCredential credential, CancellationToken cancellationToken = default)
    {
        _context.Set<WebAuthnCredential>().Update(credential);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(WebAuthnCredential credential, CancellationToken cancellationToken = default)
    {
        _context.Set<WebAuthnCredential>().Remove(credential);
        await Task.CompletedTask;
    }
}

