using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Session.Domain.Sessions.Events;

/// <summary>
/// Event raised when a quiz session is created
/// </summary>
public sealed record SessionCreatedEvent : DomainEvent
{
    public Guid SessionId { get; init; }
    public Guid QuizId { get; init; }
    public Guid HostId { get; init; }
    public string SessionCode { get; init; }
    public int MaxPlayers { get; init; }

    public SessionCreatedEvent(Guid sessionId, Guid quizId, Guid hostId, string sessionCode, int maxPlayers)
    {
        SessionId = sessionId;
        QuizId = quizId;
        HostId = hostId;
        SessionCode = sessionCode;
        MaxPlayers = maxPlayers;
    }
}
