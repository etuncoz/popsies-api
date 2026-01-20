using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Quiz.Domain.Categories.Events;

/// <summary>
/// Domain event raised when a category is deactivated
/// </summary>
public sealed record CategoryDeactivatedEvent : DomainEvent
{
    public Guid CategoryId { get; init; }
    public string Name { get; init; }

    public CategoryDeactivatedEvent(Guid categoryId, string name)
    {
        CategoryId = categoryId;
        Name = name;
    }
}
