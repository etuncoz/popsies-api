using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Identity.Core.Domain.Events;

public sealed record GuestCreatedEvent : DomainEvent
{
    public Guid GuestId { get; init; }
    public string DisplayName { get; init; }

    public GuestCreatedEvent(Guid guestId, string displayName)
    {
        GuestId = guestId;
        DisplayName = displayName;
    }
}
