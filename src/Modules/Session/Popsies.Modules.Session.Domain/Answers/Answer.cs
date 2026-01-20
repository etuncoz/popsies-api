using Popsies.Shared.Abstractions.Domain;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Session.Domain.Answers;

/// <summary>
/// Answer entity - represents a player's answer to a question
/// Invariants:
/// - Time taken cannot be negative
/// - Answer can only be submitted once
/// </summary>
public sealed class Answer : Entity
{
    public Guid SessionId { get; private set; }
    public Guid PlayerId { get; private set; }
    public Guid QuestionId { get; private set; }
    public Guid SelectedItemId { get; private set; }
    public bool IsCorrect { get; private set; }
    public int PointsEarned { get; private set; }
    public int TimeTakenSeconds { get; private set; }
    public DateTime SubmittedAt { get; private set; }

    private Answer() { }

    private Answer(
        Guid id,
        Guid sessionId,
        Guid playerId,
        Guid questionId,
        Guid selectedItemId,
        bool isCorrect,
        int pointsEarned,
        int timeTakenSeconds,
        DateTime submittedAt) : base(id)
    {
        SessionId = sessionId;
        PlayerId = playerId;
        QuestionId = questionId;
        SelectedItemId = selectedItemId;
        IsCorrect = isCorrect;
        PointsEarned = pointsEarned;
        TimeTakenSeconds = timeTakenSeconds;
        SubmittedAt = submittedAt;
    }

    /// <summary>
    /// Creates a new answer
    /// </summary>
    public static Result<Answer> Create(
        Guid id,
        Guid sessionId,
        Guid playerId,
        Guid questionId,
        Guid selectedItemId,
        bool isCorrect,
        int pointsEarned,
        int timeTakenSeconds)
    {
        if (sessionId == Guid.Empty)
        {
            return Result.Failure<Answer>(Error.Validation("SessionId", "Session ID cannot be empty"));
        }

        if (playerId == Guid.Empty)
        {
            return Result.Failure<Answer>(Error.Validation("PlayerId", "Player ID cannot be empty"));
        }

        if (questionId == Guid.Empty)
        {
            return Result.Failure<Answer>(Error.Validation("QuestionId", "Question ID cannot be empty"));
        }

        if (selectedItemId == Guid.Empty)
        {
            return Result.Failure<Answer>(Error.Validation("SelectedItemId", "Selected item ID cannot be empty"));
        }

        if (pointsEarned < 0)
        {
            return Result.Failure<Answer>(Error.Validation("PointsEarned", "Points earned cannot be negative"));
        }

        if (timeTakenSeconds < 0)
        {
            return Result.Failure<Answer>(Error.Validation("TimeTakenSeconds", "Time taken cannot be negative"));
        }

        var answer = new Answer(
            id,
            sessionId,
            playerId,
            questionId,
            selectedItemId,
            isCorrect,
            pointsEarned,
            timeTakenSeconds,
            DateTime.UtcNow);

        return Result.Success(answer);
    }
}
