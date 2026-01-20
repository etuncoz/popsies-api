using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Session.Domain.Players.Events;

/// <summary>
/// Event raised when a player leaves a session
/// </summary>
public sealed record PlayerLeftEvent : DomainEvent
{
    public Guid SessionId { get; init; }
    public Guid PlayerId { get; init; }
    public DateTime LeftAt { get; init; }

    public PlayerLeftEvent(Guid sessionId, Guid playerId, DateTime leftAt)
    {
        SessionId = sessionId;
        PlayerId = playerId;
        LeftAt = leftAt;
    }
}
