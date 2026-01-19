using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Identity.Core.Domain.Events;

public sealed record GuestExpiredEvent : DomainEvent
{
    public Guid GuestId { get; init; }

    public GuestExpiredEvent(Guid guestId)
    {
        GuestId = guestId;
    }
}
