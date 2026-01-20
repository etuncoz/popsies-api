namespace Popsies.Modules.Session.Contracts.Responses;

public sealed record AnswerResponse(
    Guid AnswerId,
    bool IsCorrect,
    int PointsEarned);
