using System.ComponentModel.DataAnnotations;

namespace Popsies.Modules.Quiz.Contracts.Requests;

/// <summary>
/// Request to add a question to a quiz
/// </summary>
public sealed record AddQuestionRequest
{
    /// <summary>
    /// The question text
    /// </summary>
    [Required]
    [StringLength(250, MinimumLength = 1)]
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// The display sequence of the question
    /// </summary>
    [Required]
    [Range(0, int.MaxValue)]
    public int Sequence { get; init; }

    /// <summary>
    /// The point value for a correct answer
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int PointValue { get; init; } = 100;

    /// <summary>
    /// The time limit in seconds for answering the question
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int TimeLimit { get; init; } = 30;
}
