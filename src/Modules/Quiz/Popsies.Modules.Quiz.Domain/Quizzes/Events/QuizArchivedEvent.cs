using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Quiz.Domain.Quizzes.Events;

/// <summary>
/// Domain event raised when a quiz is archived
/// </summary>
public sealed record QuizArchivedEvent : DomainEvent
{
    public Guid QuizId { get; init; }
    public string Title { get; init; }

    public QuizArchivedEvent(Guid quizId, string title)
    {
        QuizId = quizId;
        Title = title;
    }
}
