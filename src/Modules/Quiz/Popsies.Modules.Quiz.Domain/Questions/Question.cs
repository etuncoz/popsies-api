using Popsies.Shared.Abstractions.Domain;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Quiz.Domain.Questions;

/// <summary>
/// Question entity - represents a quiz question with multiple choice options
/// Invariants:
/// - Text must be 1-250 characters
/// - Must have 2-5 question items
/// - Exactly one item must be marked as correct
/// - Point value must be positive
/// - Time limit must be positive
/// </summary>
public sealed class Question : Entity
{
    private const int MinTextLength = 1;
    private const int MaxTextLength = 250;
    private const int MinItemCount = 2;
    private const int MaxItemCount = 5;
    private const int DefaultPointValue = 100;
    private const int DefaultTimeLimit = 30;
    private const int DefaultHintPenalty = 50;

    private readonly List<QuestionItem> _items = new();

    public Guid QuizId { get; private set; }
    public string Text { get; private set; }
    public int Sequence { get; private set; }
    public int PointValue { get; private set; }
    public int TimeLimit { get; private set; }
    public string? HintText { get; private set; }
    public int HintPenalty { get; private set; }
    public string? ImageUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<QuestionItem> QuestionItems => _items.AsReadOnly();

    private Question() { }

    private Question(Guid id, Guid quizId, string text, int sequence, int pointValue, int timeLimit) : base(id)
    {
        QuizId = quizId;
        Text = text;
        Sequence = sequence;
        PointValue = pointValue;
        TimeLimit = timeLimit;
        HintPenalty = DefaultHintPenalty;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new question
    /// </summary>
    /// <param name="id">The question ID</param>
    /// <param name="quizId">The quiz ID this question belongs to</param>
    /// <param name="text">The question text</param>
    /// <param name="sequence">The display sequence</param>
    /// <param name="pointValue">The point value for correct answer (default: 100)</param>
    /// <param name="timeLimit">The time limit in seconds (default: 30)</param>
    /// <returns>Result containing the question or validation errors</returns>
    public static Result<Question> Create(
        Guid id,
        Guid quizId,
        string text,
        int sequence,
        int pointValue = DefaultPointValue,
        int timeLimit = DefaultTimeLimit)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure<Question>(Error.Validation("Text", "Question text cannot be empty"));
        }

        if (text.Length < MinTextLength || text.Length > MaxTextLength)
        {
            return Result.Failure<Question>(Error.Validation("Text",
                $"Question text must be {MinTextLength}-{MaxTextLength} characters long"));
        }

        if (sequence < 0)
        {
            return Result.Failure<Question>(Error.Validation("Sequence", "Sequence must be non-negative"));
        }

        if (pointValue <= 0)
        {
            return Result.Failure<Question>(Error.Validation("PointValue", "Point value must be positive"));
        }

        if (timeLimit <= 0)
        {
            return Result.Failure<Question>(Error.Validation("TimeLimit", "Time limit must be positive"));
        }

        var question = new Question(id, quizId, text, sequence, pointValue, timeLimit);

        return Result.Success(question);
    }

    /// <summary>
    /// Adds a question item (answer option)
    /// </summary>
    public Result AddItem(QuestionItem item)
    {
        if (_items.Count >= MaxItemCount)
        {
            return Result.Failure(Error.Create("Question.MaxItems",
                $"Cannot add more than {MaxItemCount} items to a question"));
        }

        if (_items.Any(i => i.Id == item.Id))
        {
            return Result.Failure(Error.Create("Question.DuplicateItem",
                "Item with this ID already exists in the question"));
        }

        _items.Add(item);

        return Result.Success();
    }

    /// <summary>
    /// Removes a question item
    /// </summary>
    public Result RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
        {
            return Result.Failure(Error.Create("Question.ItemNotFound", "Question item not found"));
        }

        _items.Remove(item);

        return Result.Success();
    }

    /// <summary>
    /// Sets the correct answer item
    /// </summary>
    public Result SetCorrectItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
        {
            return Result.Failure(Error.Create("Question.ItemNotFound", "Question item not found"));
        }

        // Mark all items as incorrect first
        foreach (var i in _items)
        {
            i.MarkAsIncorrect();
        }

        // Mark the selected item as correct
        item.MarkAsCorrect();

        return Result.Success();
    }

    /// <summary>
    /// Updates the question text
    /// </summary>
    public Result UpdateText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure(Error.Validation("Text", "Question text cannot be empty"));
        }

        if (text.Length < MinTextLength || text.Length > MaxTextLength)
        {
            return Result.Failure(Error.Validation("Text",
                $"Question text must be {MinTextLength}-{MaxTextLength} characters long"));
        }

        Text = text;

        return Result.Success();
    }

    /// <summary>
    /// Sets the hint text and penalty
    /// </summary>
    public Result SetHint(string? hintText, int hintPenalty = DefaultHintPenalty)
    {
        if (hintText is not null && string.IsNullOrWhiteSpace(hintText))
        {
            return Result.Failure(Error.Validation("HintText", "Hint text cannot be empty if provided"));
        }

        if (hintPenalty < 0)
        {
            return Result.Failure(Error.Validation("HintPenalty", "Hint penalty must be non-negative"));
        }

        HintText = hintText;
        HintPenalty = hintPenalty;

        return Result.Success();
    }

    /// <summary>
    /// Sets the image URL for this question
    /// </summary>
    public Result SetImageUrl(string? imageUrl)
    {
        if (imageUrl is not null && !IsValidUrl(imageUrl))
        {
            return Result.Failure(Error.Validation("ImageUrl", "Invalid image URL. Must be a valid HTTP/HTTPS URL"));
        }

        ImageUrl = imageUrl;

        return Result.Success();
    }

    /// <summary>
    /// Validates that the question is ready for use
    /// </summary>
    public Result Validate()
    {
        if (_items.Count < MinItemCount)
        {
            return Result.Failure(Error.Validation("QuestionItems",
                $"Question must have at least {MinItemCount} answer options"));
        }

        if (_items.Count > MaxItemCount)
        {
            return Result.Failure(Error.Validation("QuestionItems",
                $"Question cannot have more than {MaxItemCount} answer options"));
        }

        var correctItemCount = _items.Count(i => i.IsCorrect);
        if (correctItemCount != 1)
        {
            return Result.Failure(Error.Validation("QuestionItems",
                "Question must have exactly one correct answer"));
        }

        return Result.Success();
    }

    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
