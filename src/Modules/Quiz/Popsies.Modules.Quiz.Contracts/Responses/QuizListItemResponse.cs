namespace Popsies.Modules.Quiz.Contracts.Responses;

/// <summary>
/// Summary quiz information for list views
/// </summary>
public sealed record QuizListItemResponse
{
    /// <summary>
    /// The quiz ID
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The quiz title
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The quiz description
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// The category ID, if assigned
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// The difficulty level (Easy, Medium, Hard)
    /// </summary>
    public string Difficulty { get; init; } = string.Empty;

    /// <summary>
    /// The current state of the quiz (Draft, Published, Archived)
    /// </summary>
    public string State { get; init; } = string.Empty;

    /// <summary>
    /// Number of questions in the quiz
    /// </summary>
    public int QuestionCount { get; init; }

    /// <summary>
    /// Total number of times the quiz has been played
    /// </summary>
    public int TotalTimesPlayed { get; init; }

    /// <summary>
    /// Average score across all plays
    /// </summary>
    public double AverageScore { get; init; }

    /// <summary>
    /// When the quiz was created
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the quiz was last updated
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}
