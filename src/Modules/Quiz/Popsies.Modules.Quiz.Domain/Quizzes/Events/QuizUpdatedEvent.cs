using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Quiz.Domain.Quizzes.Events;

/// <summary>
/// Domain event raised when quiz details are updated
/// </summary>
public sealed record QuizUpdatedEvent : DomainEvent
{
    public Guid QuizId { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }

    public QuizUpdatedEvent(Guid quizId, string title, string description)
    {
        QuizId = quizId;
        Title = title;
        Description = description;
    }
}
