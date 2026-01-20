namespace Popsies.Modules.Quiz.Contracts.Responses;

/// <summary>
/// Response after adding a question to a quiz
/// </summary>
/// <param name="QuestionId">The ID of the created question</param>
/// <param name="QuizId">The ID of the quiz the question was added to</param>
/// <param name="Text">The question text</param>
/// <param name="Sequence">The question sequence number</param>
/// <param name="Message">Success message</param>
public sealed record AddQuestionResponse(
    Guid QuestionId,
    Guid QuizId,
    string Text,
    int Sequence,
    string Message);
