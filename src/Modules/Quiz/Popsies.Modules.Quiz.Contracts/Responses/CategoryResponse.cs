namespace Popsies.Modules.Quiz.Contracts.Responses;

/// <summary>
/// Detailed category response
/// </summary>
public sealed record CategoryResponse
{
    /// <summary>
    /// The category ID
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The category name
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The category description
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Optional icon URL for the category
    /// </summary>
    public string? IconUrl { get; init; }

    /// <summary>
    /// Whether the category is active
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// The parent category ID, if this is a subcategory
    /// </summary>
    public Guid? ParentCategoryId { get; init; }

    /// <summary>
    /// The number of quizzes in this category
    /// </summary>
    public int QuizCount { get; init; }

    /// <summary>
    /// When the category was created
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the category was last updated
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}
