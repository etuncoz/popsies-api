namespace Popsies.Modules.Session.Contracts.Requests;

public sealed record CreateSessionRequest(
    Guid QuizId,
    int MaxPlayers);
