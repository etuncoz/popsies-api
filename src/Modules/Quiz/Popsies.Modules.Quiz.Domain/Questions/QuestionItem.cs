using Popsies.Shared.Abstractions.Domain;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Quiz.Domain.Questions;

/// <summary>
/// Question item entity - represents an answer option for a question
/// Invariants:
/// - Text must be 1-100 characters
/// - Only one item per question can be marked as correct
/// </summary>
public sealed class QuestionItem : Entity
{
    private const int MinTextLength = 1;
    private const int MaxTextLength = 100;

    public Guid QuestionId { get; private set; }
    public string Text { get; private set; }
    public bool IsCorrect { get; private set; }
    public int Sequence { get; private set; }
    public string? ImageUrl { get; private set; }

    private QuestionItem() { }

    private QuestionItem(Guid id, Guid questionId, string text, bool isCorrect, int sequence) : base(id)
    {
        QuestionId = questionId;
        Text = text;
        IsCorrect = isCorrect;
        Sequence = sequence;
    }

    /// <summary>
    /// Creates a new question item
    /// </summary>
    /// <param name="id">The question item ID</param>
    /// <param name="questionId">The question ID this item belongs to</param>
    /// <param name="text">The item text</param>
    /// <param name="isCorrect">Whether this is the correct answer</param>
    /// <param name="sequence">The display sequence</param>
    /// <returns>Result containing the question item or validation errors</returns>
    public static Result<QuestionItem> Create(Guid id, Guid questionId, string text, bool isCorrect, int sequence)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure<QuestionItem>(Error.Validation("Text", "Question item text cannot be empty"));
        }

        if (text.Length < MinTextLength || text.Length > MaxTextLength)
        {
            return Result.Failure<QuestionItem>(Error.Validation("Text",
                $"Question item text must be {MinTextLength}-{MaxTextLength} characters long"));
        }

        if (sequence < 0)
        {
            return Result.Failure<QuestionItem>(Error.Validation("Sequence", "Sequence must be non-negative"));
        }

        var item = new QuestionItem(id, questionId, text, isCorrect, sequence);

        return Result.Success(item);
    }

    /// <summary>
    /// Updates the item text
    /// </summary>
    public Result UpdateText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure(Error.Validation("Text", "Question item text cannot be empty"));
        }

        if (text.Length < MinTextLength || text.Length > MaxTextLength)
        {
            return Result.Failure(Error.Validation("Text",
                $"Question item text must be {MinTextLength}-{MaxTextLength} characters long"));
        }

        Text = text;

        return Result.Success();
    }

    /// <summary>
    /// Sets the image URL for this item
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
    /// Marks this item as correct
    /// </summary>
    internal void MarkAsCorrect()
    {
        IsCorrect = true;
    }

    /// <summary>
    /// Marks this item as incorrect
    /// </summary>
    internal void MarkAsIncorrect()
    {
        IsCorrect = false;
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
