using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Quiz.Application.UseCases.PublishQuiz;

/// <summary>
/// Command to publish a quiz
/// </summary>
/// <param name="QuizId">The ID of the quiz to publish</param>
public sealed record PublishQuizCommand(Guid QuizId) : ICommand;
