using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for OAuth2 authorization code operations
/// </summary>
public class AuthorizationCodeRepository : IAuthorizationCodeRepository
{
    private readonly ApplicationDbContext _context;

    public AuthorizationCodeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AuthorizationCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.AuthorizationCodes
            .FirstOrDefaultAsync(ac => ac.Code == code, cancellationToken);
    }

    public async Task AddAsync(AuthorizationCode authorizationCode, CancellationToken cancellationToken = default)
    {
        await _context.AuthorizationCodes.AddAsync(authorizationCode, cancellationToken);
    }

    public async Task UpdateAsync(AuthorizationCode authorizationCode, CancellationToken cancellationToken = default)
    {
        _context.AuthorizationCodes.Update(authorizationCode);
        await Task.CompletedTask;
    }

    public async Task DeleteExpiredCodesAsync(CancellationToken cancellationToken = default)
    {
        var expiredCodes = await _context.AuthorizationCodes
            .Where(ac => ac.ExpiresAt < DateTime.UtcNow || ac.IsUsed)
            .ToListAsync(cancellationToken);

        _context.AuthorizationCodes.RemoveRange(expiredCodes);
    }
}

