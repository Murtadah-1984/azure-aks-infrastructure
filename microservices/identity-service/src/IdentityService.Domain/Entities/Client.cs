using IdentityService.Domain.DomainEvents;

namespace IdentityService.Domain.Entities;

/// <summary>
/// OAuth2 client entity representing registered applications
/// </summary>
public class Client : AggregateRoot
{
    public string ClientId { get; private set; } = string.Empty;
    public string ClientSecretHash { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public bool RequireConsent { get; private set; }
    public bool RequirePkce { get; private set; }
    public int AccessTokenLifetime { get; private set; } // in seconds
    public int RefreshTokenLifetime { get; private set; } // in days
    public string AllowedGrantTypes { get; private set; } = string.Empty; // JSON array
    public string AllowedScopes { get; private set; } = string.Empty; // JSON array
    public string RedirectUris { get; private set; } = string.Empty; // JSON array
    public string PostLogoutRedirectUris { get; private set; } = string.Empty; // JSON array
    public DateTime? LastUsedAt { get; private set; }
    public Guid? CreatedByUserId { get; private set; }

    private Client() { } // EF Core

    private Client(
        Guid id,
        string clientId,
        string clientSecretHash,
        string name,
        string? description = null,
        bool requireConsent = true,
        bool requirePkce = false,
        int accessTokenLifetime = 3600,
        int refreshTokenLifetime = 30)
    {
        Id = id;
        ClientId = clientId;
        ClientSecretHash = clientSecretHash;
        Name = name;
        Description = description;
        IsActive = true;
        RequireConsent = requireConsent;
        RequirePkce = requirePkce;
        AccessTokenLifetime = accessTokenLifetime;
        RefreshTokenLifetime = refreshTokenLifetime;
        AllowedGrantTypes = "[]";
        AllowedScopes = "[]";
        RedirectUris = "[]";
        PostLogoutRedirectUris = "[]";
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ClientCreatedEvent(Id, ClientId, Name));
    }

    public static Client Create(
        string clientId,
        string clientSecretHash,
        string name,
        string? description = null,
        bool requireConsent = true,
        bool requirePkce = false,
        int accessTokenLifetime = 3600,
        int refreshTokenLifetime = 30,
        Guid? createdByUserId = null)
    {
        var client = new Client(
            Guid.NewGuid(),
            clientId,
            clientSecretHash,
            name,
            description,
            requireConsent,
            requirePkce,
            accessTokenLifetime,
            refreshTokenLifetime)
        {
            CreatedByUserId = createdByUserId
        };

        return client;
    }

    public void Update(
        string? name = null,
        string? description = null,
        bool? requireConsent = null,
        bool? requirePkce = null,
        int? accessTokenLifetime = null,
        int? refreshTokenLifetime = null)
    {
        if (!string.IsNullOrEmpty(name))
            Name = name;
        
        Description = description ?? Description;
        
        if (requireConsent.HasValue)
            RequireConsent = requireConsent.Value;
        
        if (requirePkce.HasValue)
            RequirePkce = requirePkce.Value;
        
        if (accessTokenLifetime.HasValue)
            AccessTokenLifetime = accessTokenLifetime.Value;
        
        if (refreshTokenLifetime.HasValue)
            RefreshTokenLifetime = refreshTokenLifetime.Value;
        
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetAllowedGrantTypes(List<string> grantTypes)
    {
        AllowedGrantTypes = System.Text.Json.JsonSerializer.Serialize(grantTypes);
        LastModifiedAt = DateTime.UtcNow;
    }

    public List<string> GetAllowedGrantTypes()
    {
        if (string.IsNullOrEmpty(AllowedGrantTypes))
            return new List<string>();
        
        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(AllowedGrantTypes) ?? new List<string>();
    }

    public void SetAllowedScopes(List<string> scopes)
    {
        AllowedScopes = System.Text.Json.JsonSerializer.Serialize(scopes);
        LastModifiedAt = DateTime.UtcNow;
    }

    public List<string> GetAllowedScopes()
    {
        if (string.IsNullOrEmpty(AllowedScopes))
            return new List<string>();
        
        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(AllowedScopes) ?? new List<string>();
    }

    public void SetRedirectUris(List<string> redirectUris)
    {
        RedirectUris = System.Text.Json.JsonSerializer.Serialize(redirectUris);
        LastModifiedAt = DateTime.UtcNow;
    }

    public List<string> GetRedirectUris()
    {
        if (string.IsNullOrEmpty(RedirectUris))
            return new List<string>();
        
        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(RedirectUris) ?? new List<string>();
    }

    public void SetPostLogoutRedirectUris(List<string> postLogoutRedirectUris)
    {
        PostLogoutRedirectUris = System.Text.Json.JsonSerializer.Serialize(postLogoutRedirectUris);
        LastModifiedAt = DateTime.UtcNow;
    }

    public List<string> GetPostLogoutRedirectUris()
    {
        if (string.IsNullOrEmpty(PostLogoutRedirectUris))
            return new List<string>();
        
        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(PostLogoutRedirectUris) ?? new List<string>();
    }

    public bool IsGrantTypeAllowed(string grantType)
    {
        return GetAllowedGrantTypes().Contains(grantType, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsScopeAllowed(string scope)
    {
        return GetAllowedScopes().Contains(scope, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsRedirectUriAllowed(string redirectUri)
    {
        return GetRedirectUris().Any(uri => 
            string.Equals(uri, redirectUri, StringComparison.OrdinalIgnoreCase) ||
            (uri.EndsWith("*") && redirectUri.StartsWith(uri.TrimEnd('*'), StringComparison.OrdinalIgnoreCase)));
    }

    public void Activate()
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RotateSecret(string newClientSecretHash)
    {
        ClientSecretHash = newClientSecretHash;
        LastModifiedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ClientSecretRotatedEvent(Id, ClientId));
    }

    public void RecordUsage()
    {
        LastUsedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }
}

