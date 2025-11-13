using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for SigningKey entity
/// </summary>
public class SigningKeyConfiguration : IEntityTypeConfiguration<SigningKey>
{
    public void Configure(EntityTypeBuilder<SigningKey> builder)
    {
        builder.ToTable("SigningKeys");

        builder.HasKey(k => k.Id);

        builder.Property(k => k.KeyId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(k => k.KeyMaterial)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(k => k.Algorithm)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(k => k.KeyId)
            .IsUnique();

        builder.HasIndex(k => k.IsActive);
        builder.HasIndex(k => new { k.IsActive, k.ExpiresAt });
    }
}

