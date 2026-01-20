using MediatR;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Quiz.Application.UseCases.DeleteQuiz;

/// <summary>
/// Handler for deleting a quiz
/// </summary>
public sealed class DeleteQuizCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteQuizCommand, Result>
{
    private readonly IQuizRepository _quizRepository = quizRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    /// Handles the delete quiz command
    /// </summary>
    /// <param name="request">The delete quiz command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result> Handle(DeleteQuizCommand request, CancellationToken cancellationToken)
    {
        // Get quiz
        var quiz = await _quizRepository.GetByIdAsync(request.QuizId, cancellationToken);
        if (quiz is null)
        {
            return Result.Failure(Error.NotFound("Quiz", request.QuizId));
        }

        // Ensure quiz is not published (only draft quizzes can be deleted)
        if (quiz.State == QuizState.Published)
        {
            return Result.Failure(Error.Create("Quiz.CannotDelete",
                "Published quizzes cannot be deleted. Archive the quiz instead."));
        }

        // Remove quiz
        _quizRepository.Remove(quiz);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Create("Database.SaveFailed", $"Failed to delete quiz: {ex.Message}"));
        }

        return Result.Success();
    }
}
