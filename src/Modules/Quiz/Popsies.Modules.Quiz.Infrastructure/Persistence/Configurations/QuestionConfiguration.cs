using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popsies.Modules.Quiz.Domain.Questions;

namespace Popsies.Modules.Quiz.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for Question entity
/// </summary>
internal sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("questions", "quiz");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id)
            .ValueGeneratedNever();

        builder.Property(q => q.QuizId)
            .IsRequired();

        builder.Property(q => q.Text)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(q => q.Sequence)
            .IsRequired();

        builder.Property(q => q.PointValue)
            .IsRequired();

        builder.Property(q => q.TimeLimit)
            .IsRequired();

        builder.Property(q => q.HintText)
            .HasMaxLength(500);

        builder.Property(q => q.HintPenalty)
            .IsRequired();

        builder.Property(q => q.ImageUrl)
            .HasMaxLength(500);

        builder.Property(q => q.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasMany(q => q.QuestionItems)
            .WithOne()
            .HasForeignKey(item => item.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(q => q.QuizId)
            .HasDatabaseName("IX_Questions_QuizId");

        builder.HasIndex(q => q.Sequence)
            .HasDatabaseName("IX_Questions_Sequence");
    }
}
