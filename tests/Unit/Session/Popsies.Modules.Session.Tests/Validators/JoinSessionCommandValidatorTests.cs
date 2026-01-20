using FluentAssertions;
using Popsies.Modules.Session.Application.UseCases.JoinSession;

namespace Popsies.Modules.Session.Tests.Unit.Validators;

public sealed class JoinSessionCommandValidatorTests
{
    private readonly JoinSessionCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new JoinSessionCommand(
            SessionCode: "ABC123",
            UserId: Guid.NewGuid(),
            DisplayName: "Player1");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_ShouldFail_WhenSessionCodeIsEmpty(string? sessionCode)
    {
        // Arrange
        var command = new JoinSessionCommand(
            SessionCode: sessionCode!,
            UserId: Guid.NewGuid(),
            DisplayName: "Player1");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SessionCode");
    }

    [Theory]
    [InlineData("ABC12")] // Too short
    [InlineData("ABC1234")] // Too long
    public void Validate_ShouldFail_WhenSessionCodeInvalidLength(string sessionCode)
    {
        // Arrange
        var command = new JoinSessionCommand(
            SessionCode: sessionCode,
            UserId: Guid.NewGuid(),
            DisplayName: "Player1");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "SessionCode");
    }

    [Fact]
    public void Validate_ShouldFail_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new JoinSessionCommand(
            SessionCode: "ABC123",
            UserId: Guid.Empty,
            DisplayName: "Player1");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "UserId");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_ShouldFail_WhenDisplayNameIsEmpty(string? displayName)
    {
        // Arrange
        var command = new JoinSessionCommand(
            SessionCode: "ABC123",
            UserId: Guid.NewGuid(),
            DisplayName: displayName!);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayName");
    }

    [Fact]
    public void Validate_ShouldFail_WhenDisplayNameTooLong()
    {
        // Arrange
        var command = new JoinSessionCommand(
            SessionCode: "ABC123",
            UserId: Guid.NewGuid(),
            DisplayName: new string('A', 51));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "DisplayName");
    }
}
