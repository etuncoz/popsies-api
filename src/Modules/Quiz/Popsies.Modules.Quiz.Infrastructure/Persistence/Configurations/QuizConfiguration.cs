using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popsies.Modules.Quiz.Domain.Categories;
using Popsies.Modules.Quiz.Domain.Quizzes;

namespace Popsies.Modules.Quiz.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for Quiz aggregate root
/// </summary>
internal sealed class QuizConfiguration : IEntityTypeConfiguration<Domain.Quizzes.Quiz>
{
    public void Configure(EntityTypeBuilder<Domain.Quizzes.Quiz> builder)
    {
        builder.ToTable("quizzes", "quiz");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id)
            .ValueGeneratedNever();

        builder.Property(q => q.CreatorId)
            .IsRequired();

        builder.Property(q => q.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(q => q.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(q => q.State)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(q => q.Difficulty)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(q => q.CategoryId);

        builder.Property(q => q.TotalTimesPlayed)
            .IsRequired();

        builder.Property(q => q.AverageScore)
            .IsRequired();

        builder.Property(q => q.CreatedAt)
            .IsRequired();

        builder.Property(q => q.UpdatedAt);

        // Relationships
        builder.HasMany(q => q.Questions)
            .WithOne()
            .HasForeignKey(question => question.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(q => q.CategoryId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(q => q.CreatorId)
            .HasDatabaseName("IX_Quizzes_CreatorId");

        builder.HasIndex(q => q.CategoryId)
            .HasDatabaseName("IX_Quizzes_CategoryId");

        builder.HasIndex(q => q.State)
            .HasDatabaseName("IX_Quizzes_State");
    }
}
