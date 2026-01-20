namespace Popsies.Modules.Session.Contracts.Responses;

public sealed record SessionResponse(
    Guid SessionId,
    string SessionCode,
    string State,
    int PlayerCount,
    int MaxPlayers,
    int CurrentQuestionIndex,
    int TotalQuestions);
