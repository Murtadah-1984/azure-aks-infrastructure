using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Client entity
/// </summary>
public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ClientId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.ClientSecretHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.AllowedGrantTypes)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.AllowedScopes)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.RedirectUris)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.PostLogoutRedirectUris)
            .HasMaxLength(2000);

        builder.HasIndex(c => c.ClientId)
            .IsUnique();

        builder.HasIndex(c => c.IsActive);
    }
}

