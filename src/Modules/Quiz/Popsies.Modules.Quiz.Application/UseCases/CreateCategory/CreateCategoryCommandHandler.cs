using MediatR;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Modules.Quiz.Domain.Categories;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Quiz.Application.UseCases.CreateCategory;

/// <summary>
/// Handler for creating a new category
/// </summary>
public sealed class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    /// Handles the create category command
    /// </summary>
    /// <param name="request">The create category command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the new category ID or error</returns>
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Check name uniqueness
        if (await _categoryRepository.ExistsByNameAsync(request.Name, cancellationToken))
        {
            return Result.Failure<Guid>(Error.Conflict("A category with this name already exists"));
        }

        // Validate parent category exists if provided
        if (request.ParentCategoryId.HasValue)
        {
            var parentCategory = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
            if (parentCategory is null)
            {
                return Result.Failure<Guid>(Error.NotFound("Category", request.ParentCategoryId.Value));
            }

            if (!parentCategory.IsActive)
            {
                return Result.Failure<Guid>(Error.Validation("ParentCategoryId", "Cannot create subcategory under inactive parent category"));
            }
        }

        // Create category aggregate
        var categoryId = Guid.NewGuid();
        var categoryResult = Category.Create(
            categoryId,
            request.Name,
            request.Description,
            request.ParentCategoryId);

        if (categoryResult.IsFailure)
        {
            return Result.Failure<Guid>(categoryResult.Error);
        }

        var category = categoryResult.Value;

        // Persist category
        await _categoryRepository.AddAsync(category, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(Error.Create("Database.SaveFailed", $"Failed to save category: {ex.Message}"));
        }

        return Result<Guid>.Success(categoryId);
    }
}
