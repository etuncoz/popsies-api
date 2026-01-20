using Popsies.Modules.Quiz.Domain.Quizzes;

namespace Popsies.Modules.Quiz.Application.Common.Repositories;

/// <summary>
/// Repository interface for Quiz aggregate operations
/// </summary>
public interface IQuizRepository
{
    /// <summary>
    /// Gets a quiz by its unique identifier
    /// </summary>
    /// <param name="id">The quiz ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The quiz if found, otherwise null</returns>
    Task<Domain.Quizzes.Quiz?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all quizzes
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all quizzes</returns>
    Task<IEnumerable<Domain.Quizzes.Quiz>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all quizzes created by a specific user
    /// </summary>
    /// <param name="creatorId">The creator user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of quizzes created by the user</returns>
    Task<IEnumerable<Domain.Quizzes.Quiz>> GetByCreatorIdAsync(Guid creatorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all quizzes in a specific category
    /// </summary>
    /// <param name="categoryId">The category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of quizzes in the category</returns>
    Task<IEnumerable<Domain.Quizzes.Quiz>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a quiz exists by ID
    /// </summary>
    /// <param name="id">The quiz ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the quiz exists, otherwise false</returns>
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new quiz to the repository
    /// </summary>
    /// <param name="quiz">The quiz to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(Domain.Quizzes.Quiz quiz, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing quiz
    /// </summary>
    /// <param name="quiz">The quiz to update</param>
    void Update(Domain.Quizzes.Quiz quiz);

    /// <summary>
    /// Removes a quiz from the repository
    /// </summary>
    /// <param name="quiz">The quiz to remove</param>
    void Remove(Domain.Quizzes.Quiz quiz);
}
