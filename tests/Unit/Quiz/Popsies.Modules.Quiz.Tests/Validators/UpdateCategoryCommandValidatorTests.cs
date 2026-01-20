using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Categories;
using FluentAssertions;
using Popsies.Modules.Quiz.Application.UseCases.UpdateCategory;

namespace Popsies.Modules.Quiz.Tests.Unit.Validators;

public sealed class UpdateCategoryCommandValidatorTests
{
    private readonly UpdateCategoryCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Science",
            Description: "Science related quizzes");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenCategoryIdIsEmpty()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            CategoryId: Guid.Empty,
            Name: "Science",
            Description: "Description");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "CategoryId");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_ShouldFail_WhenNameIsEmpty(string? name)
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            CategoryId: Guid.NewGuid(),
            Name: name!,
            Description: "Description");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Name");
        result.Errors.First().ErrorMessage.Should().Contain("Category name is required");
    }

    [Fact]
    public void Validate_ShouldFail_WhenNameExceedsMaxLength()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            CategoryId: Guid.NewGuid(),
            Name: new string('A', 101),
            Description: "Description");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Name");
        result.Errors.First().ErrorMessage.Should().Contain("100 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_ShouldFail_WhenDescriptionIsEmpty(string? description)
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Science",
            Description: description!);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Description");
        result.Errors.First().ErrorMessage.Should().Contain("Category description is required");
    }

    [Fact]
    public void Validate_ShouldFail_WhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Science",
            Description: new string('A', 501));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Description");
        result.Errors.First().ErrorMessage.Should().Contain("500 characters");
    }
}
