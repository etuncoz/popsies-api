using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Quiz.Domain.Categories.Events;

/// <summary>
/// Domain event raised when category details are updated
/// </summary>
public sealed record CategoryUpdatedEvent : DomainEvent
{
    public Guid CategoryId { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }

    public CategoryUpdatedEvent(Guid categoryId, string name, string description)
    {
        CategoryId = categoryId;
        Name = name;
        Description = description;
    }
}
