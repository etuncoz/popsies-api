using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Quiz.Application.UseCases.CreateQuiz;

/// <summary>
/// Command to create a new quiz
/// </summary>
/// <param name="CreatorId">The ID of the user creating the quiz</param>
/// <param name="Title">The quiz title</param>
/// <param name="Description">The quiz description</param>
/// <param name="CategoryId">Optional category ID</param>
/// <param name="Difficulty">The quiz difficulty level</param>
public sealed record CreateQuizCommand(
    Guid CreatorId,
    string Title,
    string Description,
    Guid? CategoryId,
    QuizDifficulty Difficulty) : ICommand<Guid>;
