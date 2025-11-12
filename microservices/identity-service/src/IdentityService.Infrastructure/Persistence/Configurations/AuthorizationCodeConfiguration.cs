using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for AuthorizationCode entity
/// </summary>
public class AuthorizationCodeConfiguration : IEntityTypeConfiguration<AuthorizationCode>
{
    public void Configure(EntityTypeBuilder<AuthorizationCode> builder)
    {
        builder.ToTable("AuthorizationCodes");

        builder.HasKey(ac => ac.Id);

        builder.Property(ac => ac.Code)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ac => ac.RedirectUri)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(ac => ac.CodeChallenge)
            .HasMaxLength(500);

        builder.Property(ac => ac.CodeChallengeMethod)
            .HasMaxLength(10);

        builder.Property(ac => ac.Scope)
            .HasMaxLength(2000);

        builder.Property(ac => ac.State)
            .HasMaxLength(500);

        builder.HasIndex(ac => ac.Code)
            .IsUnique();

        builder.HasIndex(ac => ac.ClientId);
        builder.HasIndex(ac => ac.UserId);
        builder.HasIndex(ac => ac.ExpiresAt);
        builder.HasIndex(ac => ac.IsUsed);

        // Composite index for cleanup queries
        builder.HasIndex(ac => new { ac.IsUsed, ac.ExpiresAt });
    }
}

