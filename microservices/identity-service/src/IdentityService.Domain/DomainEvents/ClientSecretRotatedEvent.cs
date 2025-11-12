namespace IdentityService.Domain.DomainEvents;

/// <summary>
/// Domain event raised when an OAuth2 client secret is rotated
/// </summary>
public class ClientSecretRotatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid ClientId { get; }
    public string ClientIdentifier { get; }

    public ClientSecretRotatedEvent(Guid clientId, string clientIdentifier)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        ClientId = clientId;
        ClientIdentifier = clientIdentifier;
    }
}

