using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Identity.Core.Domain.Events;

public sealed record GuestConvertedEvent : DomainEvent
{
    public Guid GuestId { get; init; }
    public Guid UserId { get; init; }

    public GuestConvertedEvent(Guid guestId, Guid userId)
    {
        GuestId = guestId;
        UserId = userId;
    }
}
