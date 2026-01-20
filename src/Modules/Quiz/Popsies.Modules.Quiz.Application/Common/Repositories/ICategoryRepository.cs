using Popsies.Modules.Quiz.Domain.Categories;

namespace Popsies.Modules.Quiz.Application.Common.Repositories;

/// <summary>
/// Repository interface for Category aggregate operations
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Gets a category by its unique identifier
    /// </summary>
    /// <param name="id">The category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The category if found, otherwise null</returns>
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category by its name
    /// </summary>
    /// <param name="name">The category name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The category if found, otherwise null</returns>
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active categories
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active categories</returns>
    Task<IEnumerable<Category>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all child categories for a specific parent category
    /// </summary>
    /// <param name="parentId">The parent category ID, or null for root categories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of child categories</returns>
    Task<IEnumerable<Category>> GetByParentIdAsync(Guid? parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a category with the given name exists
    /// </summary>
    /// <param name="name">The category name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if a category with the name exists, otherwise false</returns>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new category to the repository
    /// </summary>
    /// <param name="category">The category to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing category
    /// </summary>
    /// <param name="category">The category to update</param>
    void Update(Category category);

    /// <summary>
    /// Removes a category from the repository
    /// </summary>
    /// <param name="category">The category to remove</param>
    void Remove(Category category);
}
