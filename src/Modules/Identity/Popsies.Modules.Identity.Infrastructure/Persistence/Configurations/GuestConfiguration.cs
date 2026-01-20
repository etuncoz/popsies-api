using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popsies.Modules.Identity.Domain.Guests;

namespace Popsies.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    public void Configure(EntityTypeBuilder<Guest> builder)
    {
        builder.ToTable("Guests");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .ValueGeneratedNever();

        builder.Property(g => g.DisplayName)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(g => g.ExpiresAt)
            .IsRequired();

        builder.Property(g => g.IsExpired)
            .IsRequired();

        builder.Property(g => g.IsConverted)
            .IsRequired();

        builder.Property(g => g.ConvertedToUserId);

        builder.Property(g => g.ActiveQuizSessionId);

        // Indexes
        builder.HasIndex(g => g.ExpiresAt)
            .HasDatabaseName("IX_Guests_ExpiresAt");

        builder.HasIndex(g => g.IsExpired)
            .HasDatabaseName("IX_Guests_IsExpired");

        builder.HasIndex(g => g.IsConverted)
            .HasDatabaseName("IX_Guests_IsConverted");

        builder.HasIndex(g => g.ConvertedToUserId)
            .HasDatabaseName("IX_Guests_ConvertedToUserId");

        // Ignore domain events
        builder.Ignore(g => g.DomainEvents);
    }
}
