using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Quiz.Domain.Quizzes.Events;

/// <summary>
/// Domain event raised when a quiz is created
/// </summary>
public sealed record QuizCreatedEvent : DomainEvent
{
    public Guid QuizId { get; init; }
    public Guid CreatorId { get; init; }
    public string Title { get; init; }
    public QuizDifficulty Difficulty { get; init; }

    public QuizCreatedEvent(Guid quizId, Guid creatorId, string title, QuizDifficulty difficulty)
    {
        QuizId = quizId;
        CreatorId = creatorId;
        Title = title;
        Difficulty = difficulty;
    }
}
