namespace Popsies.Modules.Quiz.Contracts.Responses;

/// <summary>
/// Response for a single question answer option
/// </summary>
public sealed record QuestionItemResponse
{
    /// <summary>
    /// The question item ID
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The answer text
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Whether this is the correct answer
    /// </summary>
    public bool IsCorrect { get; init; }

    /// <summary>
    /// The display order of this answer option
    /// </summary>
    public int Order { get; init; }
}
