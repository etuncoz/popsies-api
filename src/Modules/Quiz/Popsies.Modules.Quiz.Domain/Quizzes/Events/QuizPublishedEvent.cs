using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Quiz.Domain.Quizzes.Events;

/// <summary>
/// Domain event raised when a quiz is published
/// </summary>
public sealed record QuizPublishedEvent : DomainEvent
{
    public Guid QuizId { get; init; }
    public string Title { get; init; }

    public QuizPublishedEvent(Guid quizId, string title)
    {
        QuizId = quizId;
        Title = title;
    }
}
