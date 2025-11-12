using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces;

/// <summary>
/// Repository interface for OAuth2 authorization code operations
/// </summary>
public interface IAuthorizationCodeRepository
{
    Task<AuthorizationCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(AuthorizationCode authorizationCode, CancellationToken cancellationToken = default);
    Task UpdateAsync(AuthorizationCode authorizationCode, CancellationToken cancellationToken = default);
    Task DeleteExpiredCodesAsync(CancellationToken cancellationToken = default);
}

