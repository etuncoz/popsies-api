using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Session.Domain.Sessions.Events;

/// <summary>
/// Event raised when a quiz session completes
/// </summary>
public sealed record SessionCompletedEvent : DomainEvent
{
    public Guid SessionId { get; init; }
    public int PlayerCount { get; init; }
    public DateTime CompletedAt { get; init; }

    public SessionCompletedEvent(Guid sessionId, int playerCount, DateTime completedAt)
    {
        SessionId = sessionId;
        PlayerCount = playerCount;
        CompletedAt = completedAt;
    }
}
