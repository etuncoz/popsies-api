namespace Popsies.Modules.Quiz.Contracts.Responses;

/// <summary>
/// Response after creating a new quiz
/// </summary>
/// <param name="QuizId">The ID of the created quiz</param>
/// <param name="Title">The quiz title</param>
/// <param name="State">The current state of the quiz</param>
/// <param name="Message">Success message</param>
public sealed record CreateQuizResponse(
    Guid QuizId,
    string Title,
    string State,
    string Message);
