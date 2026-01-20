using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Quizzes.Events;
using Popsies.Shared.Abstractions.Domain;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Quiz.Domain.Quizzes;

/// <summary>
/// Quiz aggregate root
/// Invariants:
/// - Title must be 3-100 characters
/// - Description must not exceed 500 characters
/// - Must have 1-10 questions
/// - Only Draft quizzes can be edited
/// - Published quizzes cannot be deleted, only archived
/// </summary>
public sealed class Quiz : AggregateRoot
{
    private const int MinTitleLength = 3;
    private const int MaxTitleLength = 100;
    private const int MaxDescriptionLength = 500;
    private const int MinQuestionCount = 1;
    private const int MaxQuestionCount = 10;

    private readonly List<Question> _questions = new();

    public Guid CreatorId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public Guid? CategoryId { get; private set; }
    public QuizDifficulty Difficulty { get; private set; }
    public QuizState State { get; private set; }
    public int TotalTimesPlayed { get; private set; }
    public double AverageScore { get; private set; }

    public IReadOnlyCollection<Question> Questions => _questions.AsReadOnly();

    private Quiz() { }

    private Quiz(Guid id, Guid creatorId, string title, string description, Guid? categoryId, QuizDifficulty difficulty)
        : base(id)
    {
        CreatorId = creatorId;
        Title = title;
        Description = description;
        CategoryId = categoryId;
        Difficulty = difficulty;
        State = QuizState.Draft;
        TotalTimesPlayed = 0;
        AverageScore = 0;
    }

    /// <summary>
    /// Creates a new quiz
    /// </summary>
    /// <param name="id">The quiz ID</param>
    /// <param name="creatorId">The creator user ID</param>
    /// <param name="title">The quiz title</param>
    /// <param name="description">The quiz description</param>
    /// <param name="categoryId">Optional category ID</param>
    /// <param name="difficulty">The quiz difficulty level</param>
    /// <returns>Result containing the quiz or validation errors</returns>
    public static Result<Quiz> Create(
        Guid id,
        Guid creatorId,
        string title,
        string description,
        Guid? categoryId,
        QuizDifficulty difficulty)
    {
        if (creatorId == Guid.Empty)
        {
            return Result.Failure<Quiz>(Error.Validation("CreatorId", "Creator ID cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure<Quiz>(Error.Validation("Title", "Quiz title cannot be empty"));
        }

        if (title.Length < MinTitleLength || title.Length > MaxTitleLength)
        {
            return Result.Failure<Quiz>(Error.Validation("Title",
                $"Quiz title must be {MinTitleLength}-{MaxTitleLength} characters long"));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<Quiz>(Error.Validation("Description", "Quiz description cannot be empty"));
        }

        if (description.Length > MaxDescriptionLength)
        {
            return Result.Failure<Quiz>(Error.Validation("Description",
                $"Quiz description must not exceed {MaxDescriptionLength} characters"));
        }

        var quiz = new Quiz(id, creatorId, title, description, categoryId, difficulty);

        quiz.RaiseDomainEvent(new QuizCreatedEvent(id, creatorId, title, difficulty));

        return Result.Success(quiz);
    }

    /// <summary>
    /// Updates the quiz details (title and description)
    /// </summary>
    public Result UpdateDetails(string title, string description)
    {
        if (State != QuizState.Draft)
        {
            return Result.Failure(Error.Create("Quiz.CannotEdit",
                "Only draft quizzes can be edited"));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure(Error.Validation("Title", "Quiz title cannot be empty"));
        }

        if (title.Length < MinTitleLength || title.Length > MaxTitleLength)
        {
            return Result.Failure(Error.Validation("Title",
                $"Quiz title must be {MinTitleLength}-{MaxTitleLength} characters long"));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure(Error.Validation("Description", "Quiz description cannot be empty"));
        }

        if (description.Length > MaxDescriptionLength)
        {
            return Result.Failure(Error.Validation("Description",
                $"Quiz description must not exceed {MaxDescriptionLength} characters"));
        }

        Title = title;
        Description = description;
        MarkAsUpdated();

        RaiseDomainEvent(new QuizUpdatedEvent(Id, title, description));

        return Result.Success();
    }

    /// <summary>
    /// Publishes the quiz, making it available for play
    /// </summary>
    public Result Publish()
    {
        if (State != QuizState.Draft)
        {
            return Result.Failure(Error.Create("Quiz.AlreadyPublished",
                "Quiz is already published or archived"));
        }

        if (_questions.Count < MinQuestionCount)
        {
            return Result.Failure(Error.Validation("Questions",
                $"Quiz must have at least {MinQuestionCount} question(s) to be published"));
        }

        if (_questions.Count > MaxQuestionCount)
        {
            return Result.Failure(Error.Validation("Questions",
                $"Quiz cannot have more than {MaxQuestionCount} questions"));
        }

        // Validate all questions
        foreach (var question in _questions)
        {
            var validationResult = question.Validate();
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }
        }

        State = QuizState.Published;
        MarkAsUpdated();

        RaiseDomainEvent(new QuizPublishedEvent(Id, Title));

        return Result.Success();
    }

    /// <summary>
    /// Archives the quiz, making it unavailable for play
    /// </summary>
    public Result Archive()
    {
        if (State == QuizState.Archived)
        {
            return Result.Failure(Error.Create("Quiz.AlreadyArchived",
                "Quiz is already archived"));
        }

        State = QuizState.Archived;
        MarkAsUpdated();

        RaiseDomainEvent(new QuizArchivedEvent(Id, Title));

        return Result.Success();
    }

    /// <summary>
    /// Adds a question to the quiz
    /// </summary>
    public Result AddQuestion(Question question)
    {
        if (State != QuizState.Draft)
        {
            return Result.Failure(Error.Create("Quiz.CannotEdit",
                "Only draft quizzes can be edited"));
        }

        if (_questions.Count >= MaxQuestionCount)
        {
            return Result.Failure(Error.Create("Quiz.MaxQuestions",
                $"Cannot add more than {MaxQuestionCount} questions to a quiz"));
        }

        if (_questions.Any(q => q.Id == question.Id))
        {
            return Result.Failure(Error.Create("Quiz.DuplicateQuestion",
                "Question with this ID already exists in the quiz"));
        }

        _questions.Add(question);
        MarkAsUpdated();

        return Result.Success();
    }

    /// <summary>
    /// Removes a question from the quiz
    /// </summary>
    public Result RemoveQuestion(Guid questionId)
    {
        if (State != QuizState.Draft)
        {
            return Result.Failure(Error.Create("Quiz.CannotEdit",
                "Only draft quizzes can be edited"));
        }

        var question = _questions.FirstOrDefault(q => q.Id == questionId);
        if (question is null)
        {
            return Result.Failure(Error.Create("Quiz.QuestionNotFound",
                "Question not found in this quiz"));
        }

        _questions.Remove(question);
        MarkAsUpdated();

        return Result.Success();
    }

    /// <summary>
    /// Updates quiz statistics after completion
    /// </summary>
    public void UpdateStatistics(int timesPlayed, double averageScore)
    {
        if (timesPlayed < 0)
        {
            timesPlayed = 0;
        }

        if (averageScore < 0)
        {
            averageScore = 0;
        }

        TotalTimesPlayed = timesPlayed;
        AverageScore = averageScore;
        MarkAsUpdated();
    }
}
