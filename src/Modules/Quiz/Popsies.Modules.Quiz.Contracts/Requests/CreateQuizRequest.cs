using System.ComponentModel.DataAnnotations;

namespace Popsies.Modules.Quiz.Contracts.Requests;

/// <summary>
/// Request to create a new quiz
/// </summary>
public sealed record CreateQuizRequest
{
    /// <summary>
    /// The quiz title
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The quiz description
    /// </summary>
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Optional category ID for the quiz
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// The difficulty level of the quiz (0 = Easy, 1 = Medium, 2 = Hard)
    /// </summary>
    [Required]
    [Range(0, 2)]
    public int Difficulty { get; init; }
}
