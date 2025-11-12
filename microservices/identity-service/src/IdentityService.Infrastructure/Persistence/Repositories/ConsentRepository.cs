using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for user consent operations
/// </summary>
public class ConsentRepository : IConsentRepository
{
    private readonly ApplicationDbContext _context;

    public ConsentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Consent?> GetByUserAndClientAsync(Guid userId, Guid clientId, CancellationToken cancellationToken = default)
    {
        return await _context.Consents
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ClientId == clientId, cancellationToken);
    }

    public async Task<List<Consent>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Consents
            .Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Consent>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await _context.Consents
            .Where(c => c.ClientId == clientId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Consent consent, CancellationToken cancellationToken = default)
    {
        await _context.Consents.AddAsync(consent, cancellationToken);
    }

    public async Task UpdateAsync(Consent consent, CancellationToken cancellationToken = default)
    {
        _context.Consents.Update(consent);
        await Task.CompletedTask;
    }

    public async Task DeleteExpiredConsentsAsync(CancellationToken cancellationToken = default)
    {
        var expiredConsents = await _context.Consents
            .Where(c => c.ExpiresAt.HasValue && c.ExpiresAt.Value < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        _context.Consents.RemoveRange(expiredConsents);
    }
}

