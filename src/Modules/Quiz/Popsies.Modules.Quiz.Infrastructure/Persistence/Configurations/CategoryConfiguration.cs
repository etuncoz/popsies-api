using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popsies.Modules.Quiz.Domain.Categories;

namespace Popsies.Modules.Quiz.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for Category aggregate root
/// </summary>
internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories", "quiz");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.IconUrl)
            .HasMaxLength(500);

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.ParentCategoryId);

        builder.Property(c => c.QuizCount)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);

        // Relationships - Self-referencing for hierarchical categories
        builder.HasMany<Category>()
            .WithOne()
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("IX_Categories_Name");

        builder.HasIndex(c => c.ParentCategoryId)
            .HasDatabaseName("IX_Categories_ParentCategoryId");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Categories_IsActive");
    }
}
