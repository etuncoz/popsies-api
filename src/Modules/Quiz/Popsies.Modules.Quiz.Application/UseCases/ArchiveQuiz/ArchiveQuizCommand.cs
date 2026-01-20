using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Quiz.Application.UseCases.ArchiveQuiz;

/// <summary>
/// Command to archive a quiz
/// </summary>
/// <param name="QuizId">The ID of the quiz to archive</param>
public sealed record ArchiveQuizCommand(Guid QuizId) : ICommand;
