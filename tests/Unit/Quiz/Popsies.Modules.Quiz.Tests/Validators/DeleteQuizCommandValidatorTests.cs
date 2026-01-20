using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Categories;
using FluentAssertions;
using Popsies.Modules.Quiz.Application.UseCases.DeleteQuiz;

namespace Popsies.Modules.Quiz.Tests.Unit.Validators;

public sealed class DeleteQuizCommandValidatorTests
{
    private readonly DeleteQuizCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new DeleteQuizCommand(QuizId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenQuizIdIsEmpty()
    {
        // Arrange
        var command = new DeleteQuizCommand(QuizId: Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "QuizId");
        result.Errors.First().ErrorMessage.Should().Contain("Quiz ID is required");
    }
}
