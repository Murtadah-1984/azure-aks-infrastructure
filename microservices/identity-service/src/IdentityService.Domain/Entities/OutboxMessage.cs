namespace IdentityService.Domain.Entities;

/// <summary>
/// Outbox message entity for reliable event publishing (Outbox Pattern)
/// </summary>
public class OutboxMessage : Entity
{
    public string MessageType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime? ProcessedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? NextRetryAt { get; private set; }

    private OutboxMessage() { } // EF Core

    public OutboxMessage(
        string messageType,
        string payload)
    {
        Id = Guid.NewGuid();
        MessageType = messageType;
        Payload = payload;
        ProcessedAt = null;
        RetryCount = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage, DateTime? nextRetryAt = null)
    {
        ErrorMessage = errorMessage;
        RetryCount++;
        NextRetryAt = nextRetryAt;
        LastModifiedAt = DateTime.UtcNow;
    }

    public bool IsProcessed()
    {
        return ProcessedAt.HasValue;
    }

    public bool ShouldRetry(int maxRetries = 5)
    {
        return !IsProcessed() && RetryCount < maxRetries && 
               (NextRetryAt == null || DateTime.UtcNow >= NextRetryAt.Value);
    }
}

