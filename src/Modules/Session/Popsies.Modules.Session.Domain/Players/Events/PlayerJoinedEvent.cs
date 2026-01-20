using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Session.Domain.Players.Events;

/// <summary>
/// Event raised when a player joins a session
/// </summary>
public sealed record PlayerJoinedEvent : DomainEvent
{
    public Guid SessionId { get; init; }
    public Guid PlayerId { get; init; }
    public Guid UserId { get; init; }
    public string DisplayName { get; init; }
    public DateTime JoinedAt { get; init; }

    public PlayerJoinedEvent(Guid sessionId, Guid playerId, Guid userId, string displayName, DateTime joinedAt)
    {
        SessionId = sessionId;
        PlayerId = playerId;
        UserId = userId;
        DisplayName = displayName;
        JoinedAt = joinedAt;
    }
}
