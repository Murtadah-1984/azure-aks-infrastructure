namespace IdentityService.Domain.DomainEvents;

/// <summary>
/// Domain event raised when a user authenticates with WebAuthn
/// </summary>
public class WebAuthnAuthenticatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid CredentialId { get; }
    public Guid UserId { get; }

    public WebAuthnAuthenticatedEvent(Guid credentialId, Guid userId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        CredentialId = credentialId;
        UserId = userId;
    }
}

