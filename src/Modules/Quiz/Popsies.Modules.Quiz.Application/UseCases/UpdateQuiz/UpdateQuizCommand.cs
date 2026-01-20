using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Quiz.Application.UseCases.UpdateQuiz;

/// <summary>
/// Command to update an existing quiz's details
/// </summary>
/// <param name="QuizId">The ID of the quiz to update</param>
/// <param name="Title">The new quiz title</param>
/// <param name="Description">The new quiz description</param>
public sealed record UpdateQuizCommand(
    Guid QuizId,
    string Title,
    string Description) : ICommand;
