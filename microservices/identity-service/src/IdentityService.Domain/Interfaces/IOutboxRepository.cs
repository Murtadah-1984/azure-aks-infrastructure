using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces;

/// <summary>
/// Repository interface for outbox message operations
/// </summary>
public interface IOutboxRepository
{
    Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize = 100, CancellationToken cancellationToken = default);
    Task<OutboxMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task DeleteProcessedMessagesAsync(DateTime beforeDate, CancellationToken cancellationToken = default);
}

