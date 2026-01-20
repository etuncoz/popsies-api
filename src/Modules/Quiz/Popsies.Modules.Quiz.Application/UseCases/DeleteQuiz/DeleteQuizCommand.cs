using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Quiz.Application.UseCases.DeleteQuiz;

/// <summary>
/// Command to delete a quiz
/// </summary>
/// <param name="QuizId">The ID of the quiz to delete</param>
public sealed record DeleteQuizCommand(Guid QuizId) : ICommand;
