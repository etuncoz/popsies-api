using FluentAssertions;
using Popsies.Modules.Session.Application.UseCases.CreateSession;

namespace Popsies.Modules.Session.Tests.Unit.Validators;

public sealed class CreateSessionCommandValidatorTests
{
    private readonly CreateSessionCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new CreateSessionCommand(
            QuizId: Guid.NewGuid(),
            HostId: Guid.NewGuid(),
            MaxPlayers: 10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenQuizIdIsEmpty()
    {
        // Arrange
        var command = new CreateSessionCommand(
            QuizId: Guid.Empty,
            HostId: Guid.NewGuid(),
            MaxPlayers: 10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "QuizId");
    }

    [Fact]
    public void Validate_ShouldFail_WhenHostIdIsEmpty()
    {
        // Arrange
        var command = new CreateSessionCommand(
            QuizId: Guid.NewGuid(),
            HostId: Guid.Empty,
            MaxPlayers: 10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "HostId");
    }

    [Fact]
    public void Validate_ShouldFail_WhenMaxPlayersTooLow()
    {
        // Arrange
        var command = new CreateSessionCommand(
            QuizId: Guid.NewGuid(),
            HostId: Guid.NewGuid(),
            MaxPlayers: 1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "MaxPlayers");
    }

    [Fact]
    public void Validate_ShouldFail_WhenMaxPlayersTooHigh()
    {
        // Arrange
        var command = new CreateSessionCommand(
            QuizId: Guid.NewGuid(),
            HostId: Guid.NewGuid(),
            MaxPlayers: 101);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "MaxPlayers");
    }
}
