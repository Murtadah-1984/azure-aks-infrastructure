namespace IdentityService.Domain.Entities;

/// <summary>
/// User consent entity for OAuth2 consent tracking
/// </summary>
public class Consent : Entity
{
    public Guid UserId { get; private set; }
    public Guid ClientId { get; private set; }
    public string Scopes { get; private set; } = string.Empty; // JSON array
    public bool IsGranted { get; private set; }
    public DateTime? GrantedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    private Consent() { } // EF Core

    public Consent(
        Guid userId,
        Guid clientId,
        List<string> scopes,
        DateTime? expiresAt = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ClientId = clientId;
        Scopes = System.Text.Json.JsonSerializer.Serialize(scopes);
        IsGranted = false;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public List<string> GetScopes()
    {
        if (string.IsNullOrEmpty(Scopes))
            return new List<string>();
        
        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(Scopes) ?? new List<string>();
    }

    public void SetScopes(List<string> scopes)
    {
        Scopes = System.Text.Json.JsonSerializer.Serialize(scopes);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Grant()
    {
        IsGranted = true;
        GrantedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Revoke()
    {
        IsGranted = false;
        RevokedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public bool IsExpired()
    {
        return ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }

    public bool IsValid()
    {
        return IsGranted && !IsExpired() && RevokedAt == null;
    }

    public bool HasScope(string scope)
    {
        return GetScopes().Contains(scope, StringComparer.OrdinalIgnoreCase);
    }
}

