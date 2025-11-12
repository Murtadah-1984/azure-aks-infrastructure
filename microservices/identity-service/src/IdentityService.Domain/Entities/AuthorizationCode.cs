namespace IdentityService.Domain.Entities;

/// <summary>
/// OAuth2 authorization code entity for authorization code flow
/// </summary>
public class AuthorizationCode : Entity
{
    public string Code { get; private set; } = string.Empty;
    public Guid ClientId { get; private set; }
    public Guid UserId { get; private set; }
    public string RedirectUri { get; private set; } = string.Empty;
    public string? CodeChallenge { get; private set; }
    public string? CodeChallengeMethod { get; private set; } // "plain" or "S256"
    public string? Scope { get; private set; }
    public string? State { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }

    private AuthorizationCode() { } // EF Core

    public AuthorizationCode(
        string code,
        Guid clientId,
        Guid userId,
        string redirectUri,
        DateTime expiresAt,
        string? codeChallenge = null,
        string? codeChallengeMethod = null,
        string? scope = null,
        string? state = null)
    {
        Id = Guid.NewGuid();
        Code = code;
        ClientId = clientId;
        UserId = userId;
        RedirectUri = redirectUri;
        CodeChallenge = codeChallenge;
        CodeChallengeMethod = codeChallengeMethod;
        Scope = scope;
        State = state;
        ExpiresAt = expiresAt;
        IsUsed = false;
        CreatedAt = DateTime.UtcNow;
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow > ExpiresAt;
    }

    public bool IsValid()
    {
        return !IsUsed && !IsExpired();
    }

    public void MarkAsUsed()
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public bool ValidateCodeVerifier(string codeVerifier)
    {
        if (string.IsNullOrEmpty(CodeChallenge))
            return true; // No PKCE required

        if (string.IsNullOrEmpty(codeVerifier))
            return false;

        if (CodeChallengeMethod == "plain")
        {
            return CodeChallenge == codeVerifier;
        }
        else if (CodeChallengeMethod == "S256")
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(codeVerifier));
            var base64Hash = Convert.ToBase64String(hash)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
            return CodeChallenge == base64Hash;
        }

        return false;
    }
}

