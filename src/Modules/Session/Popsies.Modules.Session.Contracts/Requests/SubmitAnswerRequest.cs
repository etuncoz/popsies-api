namespace Popsies.Modules.Session.Contracts.Requests;

public sealed record SubmitAnswerRequest(
    Guid QuestionId,
    Guid SelectedItemId,
    int TimeTakenSeconds);
