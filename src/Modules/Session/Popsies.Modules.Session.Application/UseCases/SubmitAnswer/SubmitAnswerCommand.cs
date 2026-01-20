using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Session.Application.UseCases.SubmitAnswer;

/// <summary>
/// Command to submit an answer to a question
/// </summary>
public sealed record SubmitAnswerCommand(
    Guid SessionId,
    Guid PlayerId,
    Guid QuestionId,
    Guid SelectedItemId,
    int TimeTakenSeconds) : ICommand<Guid>;
