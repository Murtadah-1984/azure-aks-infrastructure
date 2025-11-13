namespace IdentityService.Domain.DomainEvents;

/// <summary>
/// Domain event raised when a WebAuthn credential is registered
/// </summary>
public class WebAuthnCredentialRegisteredEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid CredentialId { get; }
    public Guid UserId { get; }
    public string CredentialIdentifier { get; }

    public WebAuthnCredentialRegisteredEvent(Guid credentialId, Guid userId, string credentialIdentifier)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        CredentialId = credentialId;
        UserId = userId;
        CredentialIdentifier = credentialIdentifier;
    }
}

