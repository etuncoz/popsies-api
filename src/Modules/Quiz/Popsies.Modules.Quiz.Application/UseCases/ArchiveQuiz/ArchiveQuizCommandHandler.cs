using MediatR;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Quiz.Application.UseCases.ArchiveQuiz;

/// <summary>
/// Handler for archiving a quiz
/// </summary>
public sealed class ArchiveQuizCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ArchiveQuizCommand, Result>
{
    private readonly IQuizRepository _quizRepository = quizRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    /// Handles the archive quiz command
    /// </summary>
    /// <param name="request">The archive quiz command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result> Handle(ArchiveQuizCommand request, CancellationToken cancellationToken)
    {
        // Get quiz
        var quiz = await _quizRepository.GetByIdAsync(request.QuizId, cancellationToken);
        if (quiz is null)
        {
            return Result.Failure(Error.NotFound("Quiz", request.QuizId));
        }

        // Archive quiz
        var archiveResult = quiz.Archive();
        if (archiveResult.IsFailure)
        {
            return archiveResult;
        }

        // Persist changes
        _quizRepository.Update(quiz);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Create("Database.SaveFailed", $"Failed to archive quiz: {ex.Message}"));
        }

        return Result.Success();
    }
}
