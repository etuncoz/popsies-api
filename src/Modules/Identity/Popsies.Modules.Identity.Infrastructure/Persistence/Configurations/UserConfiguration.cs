using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popsies.Modules.Identity.Core.Domain.Aggregates;
using Popsies.Modules.Identity.Core.Domain.ValueObjects;

namespace Popsies.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        // Username value object
        builder.OwnsOne(u => u.Username, username =>
        {
            username.Property(un => un.DisplayName)
                .HasColumnName("Username_DisplayName")
                .HasMaxLength(20)
                .IsRequired();

            username.Property(un => un.Discriminator)
                .HasColumnName("Username_Discriminator")
                .IsRequired();

            username.Ignore(un => un.FullUsername);
        });

        // Email value object
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(255)
                .IsRequired();

            email.HasIndex(e => e.Value)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");
        });

        builder.Property(u => u.KeycloakUserId)
            .HasMaxLength(100);

        builder.Property(u => u.DisplayName)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(u => u.IsDeleted)
            .IsRequired();

        builder.Property(u => u.DeletedAt);

        builder.Property(u => u.ActiveQuizSessionId);

        builder.Property(u => u.TotalQuizzesPlayed)
            .IsRequired();

        builder.Property(u => u.TotalWins)
            .IsRequired();

        builder.Property(u => u.AverageScore)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);

        // Indexes
        builder.HasIndex(u => u.IsDeleted)
            .HasDatabaseName("IX_Users_IsDeleted");

        builder.HasIndex(u => u.KeycloakUserId)
            .IsUnique()
            .HasDatabaseName("IX_Users_KeycloakUserId")
            .HasFilter("\"KeycloakUserId\" IS NOT NULL");

        // Ignore domain events
        builder.Ignore(u => u.DomainEvents);
    }
}
