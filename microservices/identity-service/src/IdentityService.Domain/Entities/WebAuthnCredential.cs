using IdentityService.Domain.DomainEvents;

namespace IdentityService.Domain.Entities;

/// <summary>
/// WebAuthn/FIDO2 credential entity
/// </summary>
public class WebAuthnCredential : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string CredentialId { get; private set; } = string.Empty; // Base64 encoded
    public string PublicKey { get; private set; } = string.Empty; // CBOR encoded public key
    public int Counter { get; private set; }
    public string? Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public string? Aaguid { get; private set; } // Authenticator Attestation Globally Unique Identifier

    private WebAuthnCredential() { } // EF Core

    private WebAuthnCredential(
        Guid id,
        Guid userId,
        string credentialId,
        string publicKey,
        int counter,
        string? name = null,
        string? aaguid = null)
    {
        Id = id;
        UserId = userId;
        CredentialId = credentialId;
        PublicKey = publicKey;
        Counter = counter;
        Name = name;
        Aaguid = aaguid;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static WebAuthnCredential Create(
        Guid userId,
        string credentialId,
        string publicKey,
        int counter,
        string? name = null,
        string? aaguid = null)
    {
        var credential = new WebAuthnCredential(
            Guid.NewGuid(),
            userId,
            credentialId,
            publicKey,
            counter,
            name,
            aaguid);

        credential.AddDomainEvent(new WebAuthnCredentialRegisteredEvent(credential.Id, userId, credentialId));
        return credential;
    }

    public void UpdateCounter(int newCounter)
    {
        if (newCounter <= Counter)
        {
            throw new InvalidOperationException("Counter must be greater than current counter");
        }

        Counter = newCounter;
        LastUsedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateName(string? name)
    {
        Name = name;
        LastModifiedAt = DateTime.UtcNow;
    }
}

