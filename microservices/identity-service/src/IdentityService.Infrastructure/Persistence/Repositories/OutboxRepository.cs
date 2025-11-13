using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for outbox message operations
/// </summary>
public class OutboxRepository : IOutboxRepository
{
    private readonly ApplicationDbContext _context;

    public OutboxRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        return await _context.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt == null)
            .Where(m => m.NextRetryAt == null || m.NextRetryAt <= DateTime.UtcNow)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<OutboxMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<OutboxMessage>()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await _context.Set<OutboxMessage>().AddAsync(message, cancellationToken);
    }

    public async Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _context.Set<OutboxMessage>().Update(message);
        await Task.CompletedTask;
    }

    public async Task DeleteProcessedMessagesAsync(DateTime beforeDate, CancellationToken cancellationToken = default)
    {
        var processedMessages = await _context.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt.HasValue && m.ProcessedAt.Value < beforeDate)
            .ToListAsync(cancellationToken);

        _context.Set<OutboxMessage>().RemoveRange(processedMessages);
    }
}

