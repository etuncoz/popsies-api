using Microsoft.EntityFrameworkCore;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Modules.Quiz.Domain.Questions;

namespace Popsies.Modules.Quiz.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Question entity operations
/// </summary>
internal sealed class QuestionRepository : IQuestionRepository
{
    private readonly QuizDbContext _context;

    public QuestionRepository(QuizDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a question by its unique identifier with all question items
    /// </summary>
    /// <param name="id">The question ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The question if found, otherwise null</returns>
    public async Task<Question?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Questions
            .Include(q => q.QuestionItems)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets all questions for a specific quiz with their question items
    /// </summary>
    /// <param name="quizId">The quiz ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of questions for the quiz</returns>
    public async Task<IEnumerable<Question>> GetByQuizIdAsync(Guid quizId, CancellationToken cancellationToken = default)
    {
        return await _context.Questions
            .Include(q => q.QuestionItems)
            .Where(q => q.QuizId == quizId)
            .OrderBy(q => q.Sequence)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a new question to the repository
    /// </summary>
    /// <param name="question">The question to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task AddAsync(Question question, CancellationToken cancellationToken = default)
    {
        await _context.Questions.AddAsync(question, cancellationToken);
    }

    /// <summary>
    /// Updates an existing question
    /// </summary>
    /// <param name="question">The question to update</param>
    public void Update(Question question)
    {
        _context.Questions.Update(question);
    }

    /// <summary>
    /// Removes a question from the repository
    /// </summary>
    /// <param name="question">The question to remove</param>
    public void Remove(Question question)
    {
        _context.Questions.Remove(question);
    }
}
