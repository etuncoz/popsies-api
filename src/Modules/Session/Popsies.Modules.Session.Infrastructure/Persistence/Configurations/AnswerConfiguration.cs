using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popsies.Modules.Session.Domain.Answers;

namespace Popsies.Modules.Session.Infrastructure.Persistence.Configurations;

internal sealed class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.ToTable("answers", "session");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.SessionId)
            .IsRequired();

        builder.Property(a => a.PlayerId)
            .IsRequired();

        builder.Property(a => a.QuestionId)
            .IsRequired();

        builder.Property(a => a.SelectedItemId)
            .IsRequired();

        builder.Property(a => a.IsCorrect)
            .IsRequired();

        builder.Property(a => a.PointsEarned)
            .IsRequired();

        builder.Property(a => a.TimeTakenSeconds)
            .IsRequired();

        builder.Property(a => a.SubmittedAt)
            .IsRequired();

        builder.HasIndex(a => new { a.SessionId, a.PlayerId, a.QuestionId })
            .IsUnique();
    }
}
