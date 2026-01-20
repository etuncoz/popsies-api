using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popsies.Modules.Identity.Domain.Accounts;

namespace Popsies.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.UserId)
            .IsRequired();

        // Username value object
        builder.OwnsOne(a => a.Username, username =>
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
        builder.OwnsOne(a => a.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(255)
                .IsRequired();

            email.HasIndex(e => e.Value)
                .IsUnique()
                .HasDatabaseName("IX_Accounts_Email");
        });

        // NOTE: Password management delegated to Keycloak

        builder.Property(a => a.State)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.IsEmailVerified)
            .IsRequired();

        builder.Property(a => a.LastLoginAt);

        // Indexes
        builder.HasIndex(a => a.UserId)
            .IsUnique()
            .HasDatabaseName("IX_Accounts_UserId");

        builder.HasIndex(a => a.State)
            .HasDatabaseName("IX_Accounts_State");
    }
}
