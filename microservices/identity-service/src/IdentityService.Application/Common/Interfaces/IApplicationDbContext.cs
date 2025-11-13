using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Application.Common.Interfaces;

/// <summary>
/// Application database context interface
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<MfaFactor> MfaFactors { get; }
    DbSet<Client> Clients { get; }
    DbSet<AuthorizationCode> AuthorizationCodes { get; }
    DbSet<Consent> Consents { get; }
    DbSet<WebAuthnCredential> WebAuthnCredentials { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

