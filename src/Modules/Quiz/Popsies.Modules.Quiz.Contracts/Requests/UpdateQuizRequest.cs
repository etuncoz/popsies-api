using System.ComponentModel.DataAnnotations;

namespace Popsies.Modules.Quiz.Contracts.Requests;

/// <summary>
/// Request to update an existing quiz's details
/// </summary>
public sealed record UpdateQuizRequest
{
    /// <summary>
    /// The new quiz title
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The new quiz description
    /// </summary>
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Description { get; init; } = string.Empty;
}
