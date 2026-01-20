using MediatR;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Quiz.Application.UseCases.AddQuestion;

/// <summary>
/// Handler for adding a question to a quiz
/// </summary>
public sealed class AddQuestionCommandHandler(
    IQuizRepository quizRepository,
    IQuestionRepository questionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AddQuestionCommand, Result<Guid>>
{
    private readonly IQuizRepository _quizRepository = quizRepository;
    private readonly IQuestionRepository _questionRepository = questionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    /// Handles the add question command
    /// </summary>
    /// <param name="request">The add question command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the new question ID or error</returns>
    public async Task<Result<Guid>> Handle(AddQuestionCommand request, CancellationToken cancellationToken)
    {
        // Get quiz
        var quiz = await _quizRepository.GetByIdAsync(request.QuizId, cancellationToken);
        if (quiz is null)
        {
            return Result.Failure<Guid>(Error.NotFound("Quiz", request.QuizId));
        }

        // Create question
        var questionId = Guid.NewGuid();
        var questionResult = Question.Create(
            questionId,
            request.QuizId,
            request.Text,
            request.Sequence,
            request.PointValue,
            request.TimeLimit);

        if (questionResult.IsFailure)
        {
            return Result.Failure<Guid>(questionResult.Error);
        }

        var question = questionResult.Value;

        // Add question to quiz
        var addResult = quiz.AddQuestion(question);
        if (addResult.IsFailure)
        {
            return Result.Failure<Guid>(addResult.Error);
        }

        // Persist question and quiz
        await _questionRepository.AddAsync(question, cancellationToken);
        _quizRepository.Update(quiz);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(Error.Create("Database.SaveFailed", $"Failed to add question: {ex.Message}"));
        }

        return Result<Guid>.Success(questionId);
    }
}
