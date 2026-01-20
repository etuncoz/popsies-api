using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popsies.Modules.Quiz.Domain.Questions;

namespace Popsies.Modules.Quiz.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for QuestionItem entity
/// </summary>
internal sealed class QuestionItemConfiguration : IEntityTypeConfiguration<QuestionItem>
{
    public void Configure(EntityTypeBuilder<QuestionItem> builder)
    {
        builder.ToTable("question_items", "quiz");

        builder.HasKey(qi => qi.Id);

        builder.Property(qi => qi.Id)
            .ValueGeneratedNever();

        builder.Property(qi => qi.QuestionId)
            .IsRequired();

        builder.Property(qi => qi.Text)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(qi => qi.IsCorrect)
            .IsRequired();

        builder.Property(qi => qi.Sequence)
            .IsRequired();

        builder.Property(qi => qi.ImageUrl)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(qi => qi.QuestionId)
            .HasDatabaseName("IX_QuestionItems_QuestionId");

        builder.HasIndex(qi => qi.Sequence)
            .HasDatabaseName("IX_QuestionItems_Sequence");
    }
}
