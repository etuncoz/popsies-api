using System.ComponentModel.DataAnnotations;

namespace Popsies.Modules.Quiz.Contracts.Requests;

/// <summary>
/// Request to create a new category
/// </summary>
public sealed record CreateCategoryRequest
{
    /// <summary>
    /// The category name
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The category description
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Optional parent category ID for hierarchical categories
    /// </summary>
    public Guid? ParentCategoryId { get; init; }
}
