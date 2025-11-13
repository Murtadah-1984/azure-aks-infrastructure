namespace IdentityService.Domain.DomainEvents;

/// <summary>
/// Domain event raised when signing keys are rotated
/// </summary>
public class SigningKeyRotatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid NewKeyId { get; }
    public string NewKeyIdentifier { get; }
    public Guid? PreviousKeyId { get; }

    public SigningKeyRotatedEvent(Guid newKeyId, string newKeyIdentifier, Guid? previousKeyId = null)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        NewKeyId = newKeyId;
        NewKeyIdentifier = newKeyIdentifier;
        PreviousKeyId = previousKeyId;
    }
}

