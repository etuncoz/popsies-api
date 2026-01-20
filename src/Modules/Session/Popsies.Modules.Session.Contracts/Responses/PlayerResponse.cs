namespace Popsies.Modules.Session.Contracts.Responses;

public sealed record PlayerResponse(
    Guid PlayerId,
    string DisplayName,
    int TotalScore,
    int CorrectAnswers,
    int Rank);
