using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Session.Domain.Answers.Events;

/// <summary>
/// Event raised when a player submits an answer
/// </summary>
public sealed record AnswerSubmittedEvent : DomainEvent
{
    public Guid SessionId { get; init; }
    public Guid PlayerId { get; init; }
    public Guid QuestionId { get; init; }
    public Guid SelectedItemId { get; init; }
    public bool IsCorrect { get; init; }
    public int PointsEarned { get; init; }
    public DateTime SubmittedAt { get; init; }

    public AnswerSubmittedEvent(
        Guid sessionId,
        Guid playerId,
        Guid questionId,
        Guid selectedItemId,
        bool isCorrect,
        int pointsEarned,
        DateTime submittedAt)
    {
        SessionId = sessionId;
        PlayerId = playerId;
        QuestionId = questionId;
        SelectedItemId = selectedItemId;
        IsCorrect = isCorrect;
        PointsEarned = pointsEarned;
        SubmittedAt = submittedAt;
    }
}
