using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Quiz.Application.UseCases.AddQuestion;

/// <summary>
/// Command to add a question to a quiz
/// </summary>
/// <param name="QuizId">The ID of the quiz to add the question to</param>
/// <param name="Text">The question text</param>
/// <param name="Sequence">The display sequence of the question</param>
/// <param name="PointValue">The point value for a correct answer</param>
/// <param name="TimeLimit">The time limit in seconds for answering the question</param>
public sealed record AddQuestionCommand(
    Guid QuizId,
    string Text,
    int Sequence,
    int PointValue,
    int TimeLimit) : ICommand<Guid>;
