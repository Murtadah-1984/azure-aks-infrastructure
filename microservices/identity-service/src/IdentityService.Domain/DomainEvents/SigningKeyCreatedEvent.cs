namespace IdentityService.Domain.DomainEvents;

/// <summary>
/// Domain event raised when a new signing key is created
/// </summary>
public class SigningKeyCreatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid KeyId { get; }
    public string KeyIdentifier { get; }

    public SigningKeyCreatedEvent(Guid keyId, string keyIdentifier)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        KeyId = keyId;
        KeyIdentifier = keyIdentifier;
    }
}

