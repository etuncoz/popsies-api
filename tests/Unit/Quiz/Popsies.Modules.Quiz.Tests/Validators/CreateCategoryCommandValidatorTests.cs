using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Categories;
using FluentAssertions;
using Popsies.Modules.Quiz.Application.UseCases.CreateCategory;

namespace Popsies.Modules.Quiz.Tests.Unit.Validators;

public sealed class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Science",
            Description: "Science related quizzes",
            ParentCategoryId: null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_ShouldFail_WhenNameIsEmpty(string? name)
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: name!,
            Description: "Description",
            ParentCategoryId: null);

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
        var command = new CreateCategoryCommand(
            Name: new string('A', 101),
            Description: "Description",
            ParentCategoryId: null);

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
        var command = new CreateCategoryCommand(
            Name: "Science",
            Description: description!,
            ParentCategoryId: null);

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
        var command = new CreateCategoryCommand(
            Name: "Science",
            Description: new string('A', 501),
            ParentCategoryId: null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Description");
        result.Errors.First().ErrorMessage.Should().Contain("500 characters");
    }
}
