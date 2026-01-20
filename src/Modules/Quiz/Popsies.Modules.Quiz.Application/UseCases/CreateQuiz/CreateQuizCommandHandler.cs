using MediatR;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Quiz.Application.UseCases.CreateQuiz;

/// <summary>
/// Handler for creating a new quiz
/// </summary>
public sealed class CreateQuizCommandHandler(
    IQuizRepository quizRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateQuizCommand, Result<Guid>>
{
    private readonly IQuizRepository _quizRepository = quizRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    /// Handles the create quiz command
    /// </summary>
    /// <param name="request">The create quiz command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the new quiz ID or error</returns>
    public async Task<Result<Guid>> Handle(CreateQuizCommand request, CancellationToken cancellationToken)
    {
        // Validate category exists if provided
        if (request.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken);
            if (category is null)
            {
                return Result.Failure<Guid>(Error.NotFound("Category", request.CategoryId.Value));
            }

            if (!category.IsActive)
            {
                return Result.Failure<Guid>(Error.Validation("CategoryId", "Cannot assign quiz to inactive category"));
            }
        }

        // Create quiz aggregate
        var quizId = Guid.NewGuid();
        var quizResult = Domain.Quizzes.Quiz.Create(
            quizId,
            request.CreatorId,
            request.Title,
            request.Description,
            request.CategoryId,
            request.Difficulty);

        if (quizResult.IsFailure)
        {
            return Result.Failure<Guid>(quizResult.Error);
        }

        var quiz = quizResult.Value;

        // Persist quiz
        await _quizRepository.AddAsync(quiz, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(Error.Create("Database.SaveFailed", $"Failed to save quiz: {ex.Message}"));
        }

        return Result<Guid>.Success(quizId);
    }
}
