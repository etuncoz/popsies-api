using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popsies.Modules.Session.Domain.Sessions;

namespace Popsies.Modules.Session.Infrastructure.Persistence.Configurations;

internal sealed class SessionConfiguration : IEntityTypeConfiguration<Domain.Sessions.Session>
{
    public void Configure(EntityTypeBuilder<Domain.Sessions.Session> builder)
    {
        builder.ToTable("sessions", "session");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.QuizId)
            .IsRequired();

        builder.Property(s => s.HostId)
            .IsRequired();

        builder.Property(s => s.SessionCode)
            .IsRequired()
            .HasMaxLength(6);

        builder.HasIndex(s => s.SessionCode)
            .IsUnique();

        builder.Property(s => s.State)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.MaxPlayers)
            .IsRequired();

        builder.Property(s => s.CurrentQuestionIndex)
            .IsRequired();

        builder.Property(s => s.TotalQuestions)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.StartedAt);

        builder.Property(s => s.CompletedAt);

        // Owned collections
        builder.HasMany(s => s.Players)
            .WithOne()
            .HasForeignKey(p => p.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Answers)
            .WithOne()
            .HasForeignKey(a => a.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(s => s.DomainEvents);
    }
}
