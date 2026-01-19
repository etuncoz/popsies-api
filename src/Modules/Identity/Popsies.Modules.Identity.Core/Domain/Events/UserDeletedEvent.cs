using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Identity.Core.Domain.Events;

public sealed record UserDeletedEvent : DomainEvent
{
    public Guid UserId { get; init; }

    public UserDeletedEvent(Guid userId)
    {
        UserId = userId;
    }
}
