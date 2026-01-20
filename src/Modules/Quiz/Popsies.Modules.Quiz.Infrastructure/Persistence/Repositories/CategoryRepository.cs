using Microsoft.EntityFrameworkCore;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Modules.Quiz.Domain.Categories;

namespace Popsies.Modules.Quiz.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Category aggregate operations
/// </summary>
internal sealed class CategoryRepository : ICategoryRepository
{
    private readonly QuizDbContext _context;

    public CategoryRepository(QuizDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a category by its unique identifier
    /// </summary>
    /// <param name="id">The category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The category if found, otherwise null</returns>
    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets a category by its name (case-insensitive)
    /// </summary>
    /// <param name="name">The category name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The category if found, otherwise null</returns>
    public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    /// <summary>
    /// Gets all active categories
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active categories</returns>
    public async Task<IEnumerable<Category>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all child categories for a specific parent category
    /// </summary>
    /// <param name="parentId">The parent category ID, or null for root categories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of child categories</returns>
    public async Task<IEnumerable<Category>> GetByParentIdAsync(Guid? parentId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.ParentCategoryId == parentId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a category with the given name exists (case-insensitive)
    /// </summary>
    /// <param name="name">The category name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if a category with the name exists, otherwise false</returns>
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AnyAsync(c => c.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    /// <summary>
    /// Adds a new category to the repository
    /// </summary>
    /// <param name="category">The category to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(category, cancellationToken);
    }

    /// <summary>
    /// Updates an existing category
    /// </summary>
    /// <param name="category">The category to update</param>
    public void Update(Category category)
    {
        _context.Categories.Update(category);
    }

    /// <summary>
    /// Removes a category from the repository
    /// </summary>
    /// <param name="category">The category to remove</param>
    public void Remove(Category category)
    {
        _context.Categories.Remove(category);
    }
}
