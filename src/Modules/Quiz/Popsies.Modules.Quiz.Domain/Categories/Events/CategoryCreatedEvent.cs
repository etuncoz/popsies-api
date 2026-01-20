using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Quiz.Domain.Categories.Events;

/// <summary>
/// Domain event raised when a category is created
/// </summary>
public sealed record CategoryCreatedEvent : DomainEvent
{
    public Guid CategoryId { get; init; }
    public string Name { get; init; }
    public Guid? ParentCategoryId { get; init; }

    public CategoryCreatedEvent(Guid categoryId, string name, Guid? parentCategoryId)
    {
        CategoryId = categoryId;
        Name = name;
        ParentCategoryId = parentCategoryId;
    }
}
