using Popsies.Modules.Quiz.Domain.Questions;

namespace Popsies.Modules.Quiz.Application.Common.Repositories;

/// <summary>
/// Repository interface for Question entity operations
/// </summary>
public interface IQuestionRepository
{
    /// <summary>
    /// Gets a question by its unique identifier
    /// </summary>
    /// <param name="id">The question ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The question if found, otherwise null</returns>
    Task<Question?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all questions for a specific quiz
    /// </summary>
    /// <param name="quizId">The quiz ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of questions for the quiz</returns>
    Task<IEnumerable<Question>> GetByQuizIdAsync(Guid quizId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new question to the repository
    /// </summary>
    /// <param name="question">The question to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(Question question, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing question
    /// </summary>
    /// <param name="question">The question to update</param>
    void Update(Question question);

    /// <summary>
    /// Removes a question from the repository
    /// </summary>
    /// <param name="question">The question to remove</param>
    void Remove(Question question);
}
