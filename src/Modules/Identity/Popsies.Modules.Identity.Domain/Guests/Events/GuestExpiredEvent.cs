using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Identity.Domain.Guests.Events;

public sealed record GuestExpiredEvent : DomainEvent
{
    public Guid GuestId { get; init; }

    public GuestExpiredEvent(Guid guestId)
    {
        GuestId = guestId;
    }
}
