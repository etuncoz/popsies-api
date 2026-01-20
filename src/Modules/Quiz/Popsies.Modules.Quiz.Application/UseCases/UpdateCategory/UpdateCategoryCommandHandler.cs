using MediatR;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Quiz.Application.UseCases.UpdateCategory;

/// <summary>
/// Handler for updating an existing category
/// </summary>
public sealed class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateCategoryCommand, Result>
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    /// Handles the update category command
    /// </summary>
    /// <param name="request">The update category command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Get category
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
        {
            return Result.Failure(Error.NotFound("Category", request.CategoryId));
        }

        // Check name uniqueness if name has changed
        if (category.Name != request.Name)
        {
            if (await _categoryRepository.ExistsByNameAsync(request.Name, cancellationToken))
            {
                return Result.Failure(Error.Conflict("A category with this name already exists"));
            }
        }

        // Update details
        var updateResult = category.UpdateDetails(request.Name, request.Description);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        // Persist changes
        _categoryRepository.Update(category);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Create("Database.SaveFailed", $"Failed to update category: {ex.Message}"));
        }

        return Result.Success();
    }
}
