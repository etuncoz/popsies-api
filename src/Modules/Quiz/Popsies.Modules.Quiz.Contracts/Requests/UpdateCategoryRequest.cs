using System.ComponentModel.DataAnnotations;

namespace Popsies.Modules.Quiz.Contracts.Requests;

/// <summary>
/// Request to update an existing category's details
/// </summary>
public sealed record UpdateCategoryRequest
{
    /// <summary>
    /// The new category name
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The new category description
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Description { get; init; } = string.Empty;
}
