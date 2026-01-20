using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Categories;
using FluentAssertions;
using Popsies.Modules.Quiz.Application.UseCases.AddQuestion;

namespace Popsies.Modules.Quiz.Tests.Unit.Validators;

public sealed class AddQuestionCommandValidatorTests
{
    private readonly AddQuestionCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new AddQuestionCommand(
            QuizId: Guid.NewGuid(),
            Text: "What is the capital of France?",
            Sequence: 1,
            PointValue: 100,
            TimeLimit: 30);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenQuizIdIsEmpty()
    {
        // Arrange
        var command = new AddQuestionCommand(
            QuizId: Guid.Empty,
            Text: "Question?",
            Sequence: 1,
            PointValue: 100,
            TimeLimit: 30);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "QuizId");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_ShouldFail_WhenTextIsEmpty(string? text)
    {
        // Arrange
        var command = new AddQuestionCommand(
            QuizId: Guid.NewGuid(),
            Text: text!,
            Sequence: 1,
            PointValue: 100,
            TimeLimit: 30);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Text");
    }

    [Fact]
    public void Validate_ShouldFail_WhenTextExceedsMaxLength()
    {
        // Arrange
        var command = new AddQuestionCommand(
            QuizId: Guid.NewGuid(),
            Text: new string('A', 501),
            Sequence: 1,
            PointValue: 100,
            TimeLimit: 30);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Text");
        result.Errors.First().ErrorMessage.Should().Contain("500 characters");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ShouldFail_WhenSequenceIsInvalid(int sequence)
    {
        // Arrange
        var command = new AddQuestionCommand(
            QuizId: Guid.NewGuid(),
            Text: "Question?",
            Sequence: sequence,
            PointValue: 100,
            TimeLimit: 30);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Sequence");
        result.Errors.First().ErrorMessage.Should().Contain("greater than 0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_ShouldFail_WhenPointValueIsInvalid(int pointValue)
    {
        // Arrange
        var command = new AddQuestionCommand(
            QuizId: Guid.NewGuid(),
            Text: "Question?",
            Sequence: 1,
            PointValue: pointValue,
            TimeLimit: 30);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "PointValue");
        result.Errors.First().ErrorMessage.Should().Contain("greater than 0");
    }

    [Fact]
    public void Validate_ShouldFail_WhenPointValueExceedsMax()
    {
        // Arrange
        var command = new AddQuestionCommand(
            QuizId: Guid.NewGuid(),
            Text: "Question?",
            Sequence: 1,
            PointValue: 10001,
            TimeLimit: 30);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "PointValue");
        result.Errors.First().ErrorMessage.Should().Contain("10000");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    public void Validate_ShouldFail_WhenTimeLimitIsInvalid(int timeLimit)
    {
        // Arrange
        var command = new AddQuestionCommand(
            QuizId: Guid.NewGuid(),
            Text: "Question?",
            Sequence: 1,
            PointValue: 100,
            TimeLimit: timeLimit);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "TimeLimit");
        result.Errors.First().ErrorMessage.Should().Contain("greater than 0");
    }

    [Fact]
    public void Validate_ShouldFail_WhenTimeLimitExceedsMax()
    {
        // Arrange
        var command = new AddQuestionCommand(
            QuizId: Guid.NewGuid(),
            Text: "Question?",
            Sequence: 1,
            PointValue: 100,
            TimeLimit: 301);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "TimeLimit");
        result.Errors.First().ErrorMessage.Should().Contain("300 seconds");
    }
}
