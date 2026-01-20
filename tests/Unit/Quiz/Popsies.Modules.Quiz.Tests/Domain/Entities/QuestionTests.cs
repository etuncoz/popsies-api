using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Categories;
using FluentAssertions;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Quiz.Tests.Unit.Domain.Entities;

public class QuestionTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateQuestion()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var text = "What is the capital of France?";
        var sequence = 1;
        var pointValue = 100;
        var timeLimit = 30;

        // Act
        var result = Question.Create(questionId, quizId, text, sequence, pointValue, timeLimit);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var question = result.Value;
        question.Id.Should().Be(questionId);
        question.QuizId.Should().Be(quizId);
        question.Text.Should().Be(text);
        question.Sequence.Should().Be(sequence);
        question.PointValue.Should().Be(pointValue);
        question.TimeLimit.Should().Be(timeLimit);
        question.HintText.Should().BeNull();
        question.HintPenalty.Should().Be(50); // Default penalty
        question.ImageUrl.Should().BeNull();
        question.QuestionItems.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithDefaultValues_ShouldUseDefaults()
    {
        // Arrange & Act
        var result = Question.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Question text?",
            1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PointValue.Should().Be(100); // Default
        result.Value.TimeLimit.Should().Be(30); // Default
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyText_ShouldReturnFailure(string? text)
    {
        // Arrange & Act
        var result = Question.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            text!,
            1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("text cannot be empty");
    }

    [Fact]
    public void Create_WithTextTooLong_ShouldReturnFailure()
    {
        // Arrange
        var text = new string('A', 251);

        // Act
        var result = Question.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            text,
            1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("1-250 characters");
    }

    [Fact]
    public void Create_WithNegativeSequence_ShouldReturnFailure()
    {
        // Arrange & Act
        var result = Question.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Question text?",
            -1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Sequence must be non-negative");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_WithInvalidPointValue_ShouldReturnFailure(int pointValue)
    {
        // Arrange & Act
        var result = Question.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Question text?",
            1,
            pointValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Point value must be positive");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    public void Create_WithInvalidTimeLimit_ShouldReturnFailure(int timeLimit)
    {
        // Arrange & Act
        var result = Question.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Question text?",
            1,
            100,
            timeLimit);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Time limit must be positive");
    }

    [Fact]
    public void AddItem_WithValidItem_ShouldAddItem()
    {
        // Arrange
        var question = CreateTestQuestion();
        var item = CreateTestQuestionItem(question.Id, "Paris", true, 1);

        // Act
        var result = question.AddItem(item);

        // Assert
        result.IsSuccess.Should().BeTrue();
        question.QuestionItems.Should().ContainSingle();
        question.QuestionItems.First().Should().Be(item);
    }

    [Fact]
    public void AddItem_WhenMaxItemsReached_ShouldReturnFailure()
    {
        // Arrange
        var question = CreateTestQuestion();
        for (int i = 0; i < 5; i++)
        {
            var item = CreateTestQuestionItem(question.Id, $"Option {i}", i == 0, i);
            question.AddItem(item);
        }
        var extraItem = CreateTestQuestionItem(question.Id, "Extra", false, 5);

        // Act
        var result = question.AddItem(extraItem);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Cannot add more than 5 items");
    }

    [Fact]
    public void AddItem_WithDuplicateId_ShouldReturnFailure()
    {
        // Arrange
        var question = CreateTestQuestion();
        var itemId = Guid.NewGuid();
        var item1 = QuestionItem.Create(itemId, question.Id, "Option 1", true, 1).Value;
        var item2 = QuestionItem.Create(itemId, question.Id, "Option 2", false, 2).Value;
        question.AddItem(item1);

        // Act
        var result = question.AddItem(item2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("already exists");
    }

    [Fact]
    public void RemoveItem_WhenItemExists_ShouldRemoveItem()
    {
        // Arrange
        var question = CreateTestQuestion();
        var item = CreateTestQuestionItem(question.Id, "Paris", true, 1);
        question.AddItem(item);

        // Act
        var result = question.RemoveItem(item.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        question.QuestionItems.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_WhenItemNotFound_ShouldReturnFailure()
    {
        // Arrange
        var question = CreateTestQuestion();

        // Act
        var result = question.RemoveItem(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("not found");
    }

    [Fact]
    public void SetCorrectItem_WhenItemExists_ShouldMarkItemAsCorrect()
    {
        // Arrange
        var question = CreateTestQuestion();
        var item1 = CreateTestQuestionItem(question.Id, "Paris", false, 1);
        var item2 = CreateTestQuestionItem(question.Id, "London", false, 2);
        question.AddItem(item1);
        question.AddItem(item2);

        // Act
        var result = question.SetCorrectItem(item1.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        item1.IsCorrect.Should().BeTrue();
        item2.IsCorrect.Should().BeFalse();
    }

    [Fact]
    public void SetCorrectItem_ShouldUnmarkPreviousCorrectItem()
    {
        // Arrange
        var question = CreateTestQuestion();
        var item1 = CreateTestQuestionItem(question.Id, "Paris", true, 1);
        var item2 = CreateTestQuestionItem(question.Id, "London", false, 2);
        question.AddItem(item1);
        question.AddItem(item2);

        // Act
        var result = question.SetCorrectItem(item2.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        item1.IsCorrect.Should().BeFalse();
        item2.IsCorrect.Should().BeTrue();
    }

    [Fact]
    public void SetCorrectItem_WhenItemNotFound_ShouldReturnFailure()
    {
        // Arrange
        var question = CreateTestQuestion();

        // Act
        var result = question.SetCorrectItem(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("not found");
    }

    [Fact]
    public void UpdateText_WithValidText_ShouldUpdateText()
    {
        // Arrange
        var question = CreateTestQuestion();
        var newText = "What is the capital of Germany?";

        // Act
        var result = question.UpdateText(newText);

        // Assert
        result.IsSuccess.Should().BeTrue();
        question.Text.Should().Be(newText);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateText_WithEmptyText_ShouldReturnFailure(string? text)
    {
        // Arrange
        var question = CreateTestQuestion();

        // Act
        var result = question.UpdateText(text!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("text cannot be empty");
    }

    [Fact]
    public void UpdateText_WithTextTooLong_ShouldReturnFailure()
    {
        // Arrange
        var question = CreateTestQuestion();
        var text = new string('A', 251);

        // Act
        var result = question.UpdateText(text);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("1-250 characters");
    }

    [Fact]
    public void SetHint_WithValidHint_ShouldSetHint()
    {
        // Arrange
        var question = CreateTestQuestion();
        var hintText = "The city is located on the Seine River";
        var hintPenalty = 25;

        // Act
        var result = question.SetHint(hintText, hintPenalty);

        // Assert
        result.IsSuccess.Should().BeTrue();
        question.HintText.Should().Be(hintText);
        question.HintPenalty.Should().Be(hintPenalty);
    }

    [Fact]
    public void SetHint_WithNull_ShouldClearHint()
    {
        // Arrange
        var question = CreateTestQuestion();
        question.SetHint("Some hint", 25);

        // Act
        var result = question.SetHint(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        question.HintText.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void SetHint_WithEmptyWhitespace_ShouldReturnFailure(string hintText)
    {
        // Arrange
        var question = CreateTestQuestion();

        // Act
        var result = question.SetHint(hintText);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Hint text cannot be empty");
    }

    [Fact]
    public void SetHint_WithNegativePenalty_ShouldReturnFailure()
    {
        // Arrange
        var question = CreateTestQuestion();

        // Act
        var result = question.SetHint("Valid hint", -10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Hint penalty must be non-negative");
    }

    [Theory]
    [InlineData("https://example.com/image.png")]
    [InlineData("http://cdn.example.com/images/france.jpg")]
    public void SetImageUrl_WithValidUrl_ShouldSetImageUrl(string imageUrl)
    {
        // Arrange
        var question = CreateTestQuestion();

        // Act
        var result = question.SetImageUrl(imageUrl);

        // Assert
        result.IsSuccess.Should().BeTrue();
        question.ImageUrl.Should().Be(imageUrl);
    }

    [Fact]
    public void SetImageUrl_WithNull_ShouldClearImageUrl()
    {
        // Arrange
        var question = CreateTestQuestion();
        question.SetImageUrl("https://example.com/image.png");

        // Act
        var result = question.SetImageUrl(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        question.ImageUrl.Should().BeNull();
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/image.png")]
    [InlineData("javascript:alert('xss')")]
    public void SetImageUrl_WithInvalidUrl_ShouldReturnFailure(string imageUrl)
    {
        // Arrange
        var question = CreateTestQuestion();

        // Act
        var result = question.SetImageUrl(imageUrl);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Invalid image URL");
    }

    [Fact]
    public void Validate_WithValidQuestion_ShouldPass()
    {
        // Arrange
        var question = CreateTestQuestionWithItems(itemCount: 3, correctItemIndex: 0);

        // Act
        var result = question.Validate();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithLessThanTwoItems_ShouldReturnFailure()
    {
        // Arrange
        var question = CreateTestQuestionWithItems(itemCount: 1, correctItemIndex: 0);

        // Act
        var result = question.Validate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("at least 2 answer options");
    }

    [Fact]
    public void Validate_WithNoCorrectItem_ShouldReturnFailure()
    {
        // Arrange
        var question = CreateTestQuestion();
        question.AddItem(CreateTestQuestionItem(question.Id, "Option 1", false, 1));
        question.AddItem(CreateTestQuestionItem(question.Id, "Option 2", false, 2));

        // Act
        var result = question.Validate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("exactly one correct answer");
    }

    [Fact]
    public void Validate_WithMultipleCorrectItems_ShouldReturnFailure()
    {
        // Arrange
        var question = CreateTestQuestion();
        question.AddItem(CreateTestQuestionItem(question.Id, "Option 1", true, 1));
        question.AddItem(CreateTestQuestionItem(question.Id, "Option 2", true, 2));

        // Act
        var result = question.Validate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("exactly one correct answer");
    }

    private static Question CreateTestQuestion()
    {
        var questionId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var result = Question.Create(
            questionId,
            quizId,
            "What is the capital of France?",
            1,
            100,
            30);

        return result.Value;
    }

    private static Question CreateTestQuestionWithItems(int itemCount, int correctItemIndex)
    {
        var question = CreateTestQuestion();

        for (int i = 0; i < itemCount; i++)
        {
            var item = CreateTestQuestionItem(
                question.Id,
                $"Option {i + 1}",
                i == correctItemIndex,
                i);
            question.AddItem(item);
        }

        return question;
    }

    private static QuestionItem CreateTestQuestionItem(Guid questionId, string text, bool isCorrect, int sequence)
    {
        var itemResult = QuestionItem.Create(
            Guid.NewGuid(),
            questionId,
            text,
            isCorrect,
            sequence);

        return itemResult.Value;
    }
}
