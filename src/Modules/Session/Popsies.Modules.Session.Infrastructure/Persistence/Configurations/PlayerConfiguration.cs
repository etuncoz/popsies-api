using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popsies.Modules.Session.Domain.Players;

namespace Popsies.Modules.Session.Infrastructure.Persistence.Configurations;

internal sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("players", "session");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.SessionId)
            .IsRequired();

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.DisplayName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.TotalScore)
            .IsRequired();

        builder.Property(p => p.CorrectAnswers)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.JoinedAt)
            .IsRequired();

        builder.Property(p => p.LeftAt);

        builder.HasIndex(p => new { p.SessionId, p.UserId });
    }
}
