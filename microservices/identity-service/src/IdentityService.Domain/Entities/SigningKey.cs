using IdentityService.Domain.DomainEvents;

namespace IdentityService.Domain.Entities;

/// <summary>
/// JWT signing key entity for key rotation
/// </summary>
public class SigningKey : AggregateRoot
{
    public string KeyId { get; private set; } = string.Empty;
    public string KeyMaterial { get; private set; } = string.Empty; // Base64 encoded key
    public string Algorithm { get; private set; } = "HS256";
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsPrevious { get; private set; } // For graceful transition

    private SigningKey() { } // EF Core

    private SigningKey(
        Guid id,
        string keyId,
        string keyMaterial,
        string algorithm,
        DateTime? expiresAt = null)
    {
        Id = id;
        KeyId = keyId;
        KeyMaterial = keyMaterial;
        Algorithm = algorithm;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        IsActive = true;
        IsPrevious = false;
    }

    public static SigningKey Create(
        string keyId,
        string keyMaterial,
        string algorithm = "HS256",
        DateTime? expiresAt = null)
    {
        var key = new SigningKey(
            Guid.NewGuid(),
            keyId,
            keyMaterial,
            algorithm,
            expiresAt);

        key.AddDomainEvent(new SigningKeyCreatedEvent(key.Id, key.KeyId));
        return key;
    }

    public void Activate()
    {
        IsActive = true;
        IsPrevious = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsPrevious()
    {
        IsPrevious = true;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public bool IsExpired()
    {
        return ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }

    public bool IsValid()
    {
        return IsActive && !IsExpired();
    }
}

