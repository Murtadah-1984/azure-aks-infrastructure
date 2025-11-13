using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for WebAuthnCredential entity
/// </summary>
public class WebAuthnCredentialConfiguration : IEntityTypeConfiguration<WebAuthnCredential>
{
    public void Configure(EntityTypeBuilder<WebAuthnCredential> builder)
    {
        builder.ToTable("WebAuthnCredentials");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CredentialId)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.PublicKey)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.Name)
            .HasMaxLength(200);

        builder.Property(c => c.Aaguid)
            .HasMaxLength(100);

        builder.HasIndex(c => c.CredentialId)
            .IsUnique();

        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.IsActive);
    }
}

