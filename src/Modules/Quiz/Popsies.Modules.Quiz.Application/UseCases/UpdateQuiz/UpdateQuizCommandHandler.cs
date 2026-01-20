using MediatR;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Quiz.Application.UseCases.UpdateQuiz;

/// <summary>
/// Handler for updating an existing quiz
/// </summary>
public sealed class UpdateQuizCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateQuizCommand, Result>
{
    private readonly IQuizRepository _quizRepository = quizRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    /// Handles the update quiz command
    /// </summary>
    /// <param name="request">The update quiz command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result> Handle(UpdateQuizCommand request, CancellationToken cancellationToken)
    {
        // Get quiz
        var quiz = await _quizRepository.GetByIdAsync(request.QuizId, cancellationToken);
        if (quiz is null)
        {
            return Result.Failure(Error.NotFound("Quiz", request.QuizId));
        }

        // Update details
        var updateResult = quiz.UpdateDetails(request.Title, request.Description);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        // Persist changes
        _quizRepository.Update(quiz);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Create("Database.SaveFailed", $"Failed to update quiz: {ex.Message}"));
        }

        return Result.Success();
    }
}
