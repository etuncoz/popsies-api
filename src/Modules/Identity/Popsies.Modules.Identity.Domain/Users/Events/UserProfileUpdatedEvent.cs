using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Identity.Domain.Users.Events;

public sealed record UserProfileUpdatedEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }

    public UserProfileUpdatedEvent(Guid userId, string? displayName, string? avatarUrl)
    {
        UserId = userId;
        DisplayName = displayName;
        AvatarUrl = avatarUrl;
    }
}
