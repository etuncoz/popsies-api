using FluentAssertions;
using Popsies.Modules.Session.Application.UseCases.SubmitAnswer;

namespace Popsies.Modules.Session.Tests.Unit.Validators;

public sealed class SubmitAnswerCommandValidatorTests
{
    private readonly SubmitAnswerCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new SubmitAnswerCommand(
            SessionId: Guid.NewGuid(),
            PlayerId: Guid.NewGuid(),
            QuestionId: Guid.NewGuid(),
            SelectedItemId: Guid.NewGuid(),
            TimeTakenSeconds: 10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenSessionIdIsEmpty()
    {
        // Arrange
        var command = new SubmitAnswerCommand(
            SessionId: Guid.Empty,
            PlayerId: Guid.NewGuid(),
            QuestionId: Guid.NewGuid(),
            SelectedItemId: Guid.NewGuid(),
            TimeTakenSeconds: 10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "SessionId");
    }

    [Fact]
    public void Validate_ShouldFail_WhenPlayerIdIsEmpty()
    {
        // Arrange
        var command = new SubmitAnswerCommand(
            SessionId: Guid.NewGuid(),
            PlayerId: Guid.Empty,
            QuestionId: Guid.NewGuid(),
            SelectedItemId: Guid.NewGuid(),
            TimeTakenSeconds: 10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "PlayerId");
    }

    [Fact]
    public void Validate_ShouldFail_WhenQuestionIdIsEmpty()
    {
        // Arrange
        var command = new SubmitAnswerCommand(
            SessionId: Guid.NewGuid(),
            PlayerId: Guid.NewGuid(),
            QuestionId: Guid.Empty,
            SelectedItemId: Guid.NewGuid(),
            TimeTakenSeconds: 10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "QuestionId");
    }

    [Fact]
    public void Validate_ShouldFail_WhenSelectedItemIdIsEmpty()
    {
        // Arrange
        var command = new SubmitAnswerCommand(
            SessionId: Guid.NewGuid(),
            PlayerId: Guid.NewGuid(),
            QuestionId: Guid.NewGuid(),
            SelectedItemId: Guid.Empty,
            TimeTakenSeconds: 10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "SelectedItemId");
    }

    [Fact]
    public void Validate_ShouldFail_WhenTimeTakenIsNegative()
    {
        // Arrange
        var command = new SubmitAnswerCommand(
            SessionId: Guid.NewGuid(),
            PlayerId: Guid.NewGuid(),
            QuestionId: Guid.NewGuid(),
            SelectedItemId: Guid.NewGuid(),
            TimeTakenSeconds: -5);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "TimeTakenSeconds");
    }
}
