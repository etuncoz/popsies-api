using Microsoft.EntityFrameworkCore;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Modules.Quiz.Domain.Quizzes;

namespace Popsies.Modules.Quiz.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Quiz aggregate operations
/// </summary>
internal sealed class QuizRepository : IQuizRepository
{
    private readonly QuizDbContext _context;

    public QuizRepository(QuizDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a quiz by its unique identifier with all related questions
    /// </summary>
    /// <param name="id">The quiz ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The quiz if found, otherwise null</returns>
    public async Task<Domain.Quizzes.Quiz?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(question => question.QuestionItems)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets all quizzes with their questions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all quizzes</returns>
    public async Task<IEnumerable<Domain.Quizzes.Quiz>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(question => question.QuestionItems)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all quizzes created by a specific user
    /// </summary>
    /// <param name="creatorId">The creator user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of quizzes created by the user</returns>
    public async Task<IEnumerable<Domain.Quizzes.Quiz>> GetByCreatorIdAsync(Guid creatorId, CancellationToken cancellationToken = default)
    {
        return await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(question => question.QuestionItems)
            .Where(q => q.CreatorId == creatorId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all quizzes in a specific category
    /// </summary>
    /// <param name="categoryId">The category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of quizzes in the category</returns>
    public async Task<IEnumerable<Domain.Quizzes.Quiz>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(question => question.QuestionItems)
            .Where(q => q.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a quiz exists by ID
    /// </summary>
    /// <param name="id">The quiz ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the quiz exists, otherwise false</returns>
    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Quizzes
            .AnyAsync(q => q.Id == id, cancellationToken);
    }

    /// <summary>
    /// Adds a new quiz to the repository
    /// </summary>
    /// <param name="quiz">The quiz to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task AddAsync(Domain.Quizzes.Quiz quiz, CancellationToken cancellationToken = default)
    {
        await _context.Quizzes.AddAsync(quiz, cancellationToken);
    }

    /// <summary>
    /// Updates an existing quiz
    /// </summary>
    /// <param name="quiz">The quiz to update</param>
    public void Update(Domain.Quizzes.Quiz quiz)
    {
        _context.Quizzes.Update(quiz);
    }

    /// <summary>
    /// Removes a quiz from the repository
    /// </summary>
    /// <param name="quiz">The quiz to remove</param>
    public void Remove(Domain.Quizzes.Quiz quiz)
    {
        _context.Quizzes.Remove(quiz);
    }
}
