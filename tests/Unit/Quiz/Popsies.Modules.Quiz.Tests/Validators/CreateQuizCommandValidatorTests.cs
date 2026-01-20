using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Categories;
using FluentAssertions;
using Popsies.Modules.Quiz.Application.UseCases.CreateQuiz;
using Popsies.Modules.Quiz.Domain.Quizzes;

namespace Popsies.Modules.Quiz.Tests.Unit.Validators;

public sealed class CreateQuizCommandValidatorTests
{
    private readonly CreateQuizCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new CreateQuizCommand(
            CreatorId: Guid.NewGuid(),
            Title: "Science Quiz",
            Description: "Test your science knowledge",
            CategoryId: Guid.NewGuid(),
            Difficulty: QuizDifficulty.Medium);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenCreatorIdIsEmpty()
    {
        // Arrange
        var command = new CreateQuizCommand(
            CreatorId: Guid.Empty,
            Title: "Quiz",
            Description: "Description",
            CategoryId: null,
            Difficulty: QuizDifficulty.Easy);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "CreatorId");
        result.Errors.First().ErrorMessage.Should().Contain("Creator ID is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_ShouldFail_WhenTitleIsEmpty(string? title)
    {
        // Arrange
        var command = new CreateQuizCommand(
            CreatorId: Guid.NewGuid(),
            Title: title!,
            Description: "Description",
            CategoryId: null,
            Difficulty: QuizDifficulty.Easy);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Title");
        result.Errors.First().ErrorMessage.Should().Contain("Title is required");
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleExceedsMaxLength()
    {
        // Arrange
        var command = new CreateQuizCommand(
            CreatorId: Guid.NewGuid(),
            Title: new string('A', 201),
            Description: "Description",
            CategoryId: null,
            Difficulty: QuizDifficulty.Easy);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Title");
        result.Errors.First().ErrorMessage.Should().Contain("200 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_ShouldFail_WhenDescriptionIsEmpty(string? description)
    {
        // Arrange
        var command = new CreateQuizCommand(
            CreatorId: Guid.NewGuid(),
            Title: "Quiz",
            Description: description!,
            CategoryId: null,
            Difficulty: QuizDifficulty.Easy);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Description");
        result.Errors.First().ErrorMessage.Should().Contain("Description is required");
    }

    [Fact]
    public void Validate_ShouldFail_WhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var command = new CreateQuizCommand(
            CreatorId: Guid.NewGuid(),
            Title: "Quiz",
            Description: new string('A', 1001),
            CategoryId: null,
            Difficulty: QuizDifficulty.Easy);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Description");
        result.Errors.First().ErrorMessage.Should().Contain("1000 characters");
    }

    [Fact]
    public void Validate_ShouldFail_WhenDifficultyIsInvalid()
    {
        // Arrange
        var command = new CreateQuizCommand(
            CreatorId: Guid.NewGuid(),
            Title: "Quiz",
            Description: "Description",
            CategoryId: null,
            Difficulty: (QuizDifficulty)999); // Invalid enum value

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Difficulty");
        result.Errors.First().ErrorMessage.Should().Contain("Invalid difficulty");
    }
}
