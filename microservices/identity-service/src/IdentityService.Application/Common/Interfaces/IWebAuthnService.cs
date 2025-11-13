namespace IdentityService.Application.Common.Interfaces;

/// <summary>
/// Service interface for WebAuthn/FIDO2 operations
/// </summary>
public interface IWebAuthnService
{
    /// <summary>
    /// Creates a registration challenge for WebAuthn credential registration
    /// </summary>
    Task<object> CreateRegistrationChallengeAsync(
        Guid userId,
        string userName,
        string userDisplayName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes WebAuthn credential registration
    /// </summary>
    Task<Guid> CompleteRegistrationAsync(
        Guid userId,
        string credentialId,
        string publicKey,
        int counter,
        byte[] attestationObject,
        byte[] clientDataJson,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an authentication challenge for WebAuthn authentication
    /// </summary>
    Task<object> CreateAuthenticationChallengeAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes WebAuthn authentication
    /// </summary>
    Task<bool> CompleteAuthenticationAsync(
        Guid userId,
        string credentialId,
        byte[] authenticatorData,
        byte[] clientDataJson,
        byte[] signature,
        int counter,
        CancellationToken cancellationToken = default);
}

