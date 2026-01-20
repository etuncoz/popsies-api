using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Categories;
using FluentAssertions;
using Popsies.Modules.Quiz.Application.UseCases.UpdateQuiz;

namespace Popsies.Modules.Quiz.Tests.Unit.Validators;

public sealed class UpdateQuizCommandValidatorTests
{
    private readonly UpdateQuizCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new UpdateQuizCommand(
            QuizId: Guid.NewGuid(),
            Title: "Updated Quiz",
            Description: "Updated description");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenQuizIdIsEmpty()
    {
        // Arrange
        var command = new UpdateQuizCommand(
            QuizId: Guid.Empty,
            Title: "Quiz",
            Description: "Description");

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
    public void Validate_ShouldFail_WhenTitleIsEmpty(string? title)
    {
        // Arrange
        var command = new UpdateQuizCommand(
            QuizId: Guid.NewGuid(),
            Title: title!,
            Description: "Description");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleExceedsMaxLength()
    {
        // Arrange
        var command = new UpdateQuizCommand(
            QuizId: Guid.NewGuid(),
            Title: new string('A', 201),
            Description: "Description");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Title");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_ShouldFail_WhenDescriptionIsEmpty(string? description)
    {
        // Arrange
        var command = new UpdateQuizCommand(
            QuizId: Guid.NewGuid(),
            Title: "Quiz",
            Description: description!);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Description");
    }

    [Fact]
    public void Validate_ShouldFail_WhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var command = new UpdateQuizCommand(
            QuizId: Guid.NewGuid(),
            Title: "Quiz",
            Description: new string('A', 1001));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Description");
    }
}
