namespace Popsies.Modules.Quiz.Contracts.Responses;

/// <summary>
/// Detailed question response with all answer options
/// </summary>
public sealed record QuestionResponse
{
    /// <summary>
    /// The question ID
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The quiz ID this question belongs to
    /// </summary>
    public Guid QuizId { get; init; }

    /// <summary>
    /// The question text
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// The display sequence of the question
    /// </summary>
    public int Sequence { get; init; }

    /// <summary>
    /// The point value for a correct answer
    /// </summary>
    public int PointValue { get; init; }

    /// <summary>
    /// The time limit in seconds for answering
    /// </summary>
    public int TimeLimit { get; init; }

    /// <summary>
    /// Optional hint text
    /// </summary>
    public string? HintText { get; init; }

    /// <summary>
    /// Point penalty for using the hint
    /// </summary>
    public int HintPenalty { get; init; }

    /// <summary>
    /// Optional image URL for the question
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// The list of answer options for this question
    /// </summary>
    public IReadOnlyCollection<QuestionItemResponse> Items { get; init; } = Array.Empty<QuestionItemResponse>();

    /// <summary>
    /// When the question was created
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
