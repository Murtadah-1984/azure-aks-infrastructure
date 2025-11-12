using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Consent entity
/// </summary>
public class ConsentConfiguration : IEntityTypeConfiguration<Consent>
{
    public void Configure(EntityTypeBuilder<Consent> builder)
    {
        builder.ToTable("Consents");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Scopes)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasIndex(c => new { c.UserId, c.ClientId })
            .IsUnique();

        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.ClientId);
        builder.HasIndex(c => c.IsGranted);
        builder.HasIndex(c => c.ExpiresAt);
    }
}

