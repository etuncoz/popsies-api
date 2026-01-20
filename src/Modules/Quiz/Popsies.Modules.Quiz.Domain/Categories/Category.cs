using Popsies.Modules.Quiz.Domain.Categories.Events;
using Popsies.Shared.Abstractions.Domain;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Quiz.Domain.Categories;

/// <summary>
/// Category aggregate root
/// Invariants:
/// - Name must be 2-50 characters and unique
/// - Description must not exceed 200 characters
/// - Quiz count must be non-negative
/// - Inactive categories cannot be assigned to new quizzes
/// </summary>
public sealed class Category : AggregateRoot
{
    private const int MinNameLength = 2;
    private const int MaxNameLength = 50;
    private const int MaxDescriptionLength = 200;

    public string Name { get; private set; }
    public string Description { get; private set; }
    public string? IconUrl { get; private set; }
    public bool IsActive { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public int QuizCount { get; private set; }

    private Category() { }

    private Category(Guid id, string name, string description, Guid? parentCategoryId) : base(id)
    {
        Name = name;
        Description = description;
        ParentCategoryId = parentCategoryId;
        IsActive = true;
        QuizCount = 0;
    }

    /// <summary>
    /// Creates a new category
    /// </summary>
    /// <param name="id">The category ID</param>
    /// <param name="name">The category name</param>
    /// <param name="description">The category description</param>
    /// <param name="parentCategoryId">Optional parent category ID for hierarchical categories</param>
    /// <returns>Result containing the category or validation errors</returns>
    public static Result<Category> Create(Guid id, string name, string description, Guid? parentCategoryId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Category>(Error.Validation("Name", "Category name cannot be empty"));
        }

        if (name.Length < MinNameLength || name.Length > MaxNameLength)
        {
            return Result.Failure<Category>(Error.Validation("Name",
                $"Category name must be {MinNameLength}-{MaxNameLength} characters long"));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<Category>(Error.Validation("Description", "Category description cannot be empty"));
        }

        if (description.Length > MaxDescriptionLength)
        {
            return Result.Failure<Category>(Error.Validation("Description",
                $"Category description must not exceed {MaxDescriptionLength} characters"));
        }

        var category = new Category(id, name, description, parentCategoryId);

        category.RaiseDomainEvent(new CategoryCreatedEvent(id, name, parentCategoryId));

        return Result.Success(category);
    }

    /// <summary>
    /// Updates the category details
    /// </summary>
    public Result UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Error.Validation("Name", "Category name cannot be empty"));
        }

        if (name.Length < MinNameLength || name.Length > MaxNameLength)
        {
            return Result.Failure(Error.Validation("Name",
                $"Category name must be {MinNameLength}-{MaxNameLength} characters long"));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure(Error.Validation("Description", "Category description cannot be empty"));
        }

        if (description.Length > MaxDescriptionLength)
        {
            return Result.Failure(Error.Validation("Description",
                $"Category description must not exceed {MaxDescriptionLength} characters"));
        }

        Name = name;
        Description = description;
        MarkAsUpdated();

        RaiseDomainEvent(new CategoryUpdatedEvent(Id, name, description));

        return Result.Success();
    }

    /// <summary>
    /// Sets the icon URL for this category
    /// </summary>
    public Result SetIconUrl(string? iconUrl)
    {
        if (iconUrl is not null && !IsValidUrl(iconUrl))
        {
            return Result.Failure(Error.Validation("IconUrl", "Invalid icon URL. Must be a valid HTTP/HTTPS URL"));
        }

        IconUrl = iconUrl;
        MarkAsUpdated();

        return Result.Success();
    }

    /// <summary>
    /// Deactivates the category
    /// </summary>
    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Failure(Error.Create("Category.AlreadyDeactivated",
                "Category is already deactivated"));
        }

        IsActive = false;
        MarkAsUpdated();

        RaiseDomainEvent(new CategoryDeactivatedEvent(Id, Name));

        return Result.Success();
    }

    /// <summary>
    /// Activates the category
    /// </summary>
    public Result Activate()
    {
        if (IsActive)
        {
            return Result.Failure(Error.Create("Category.AlreadyActive",
                "Category is already active"));
        }

        IsActive = true;
        MarkAsUpdated();

        return Result.Success();
    }

    /// <summary>
    /// Increments the quiz count for this category
    /// </summary>
    public void IncrementQuizCount()
    {
        QuizCount++;
        MarkAsUpdated();
    }

    /// <summary>
    /// Decrements the quiz count for this category
    /// </summary>
    public void DecrementQuizCount()
    {
        if (QuizCount > 0)
        {
            QuizCount--;
            MarkAsUpdated();
        }
    }

    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
