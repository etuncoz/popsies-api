using MediatR;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Quiz.Application.UseCases.PublishQuiz;

/// <summary>
/// Handler for publishing a quiz
/// </summary>
public sealed class PublishQuizCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<PublishQuizCommand, Result>
{
    private readonly IQuizRepository _quizRepository = quizRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    /// Handles the publish quiz command
    /// </summary>
    /// <param name="request">The publish quiz command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result> Handle(PublishQuizCommand request, CancellationToken cancellationToken)
    {
        // Get quiz
        var quiz = await _quizRepository.GetByIdAsync(request.QuizId, cancellationToken);
        if (quiz is null)
        {
            return Result.Failure(Error.NotFound("Quiz", request.QuizId));
        }

        // Publish quiz
        var publishResult = quiz.Publish();
        if (publishResult.IsFailure)
        {
            return publishResult;
        }

        // Persist changes
        _quizRepository.Update(quiz);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Create("Database.SaveFailed", $"Failed to publish quiz: {ex.Message}"));
        }

        return Result.Success();
    }
}
