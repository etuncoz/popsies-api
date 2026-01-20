using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Session.Domain.Sessions.Events;

/// <summary>
/// Event raised when a quiz session starts
/// </summary>
public sealed record SessionStartedEvent : DomainEvent
{
    public Guid SessionId { get; init; }
    public int PlayerCount { get; init; }
    public DateTime StartedAt { get; init; }

    public SessionStartedEvent(Guid sessionId, int playerCount, DateTime startedAt)
    {
        SessionId = sessionId;
        PlayerCount = playerCount;
        StartedAt = startedAt;
    }
}
