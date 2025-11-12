namespace IdentityService.Domain.DomainEvents;

/// <summary>
/// Domain event raised when a new OAuth2 client is created
/// </summary>
public class ClientCreatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid ClientId { get; }
    public string ClientIdentifier { get; }
    public string ClientName { get; }

    public ClientCreatedEvent(Guid id, string clientIdentifier, string clientName)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        ClientId = id;
        ClientIdentifier = clientIdentifier;
        ClientName = clientName;
    }
}

