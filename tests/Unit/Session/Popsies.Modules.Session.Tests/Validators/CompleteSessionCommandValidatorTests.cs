using FluentAssertions;
using Popsies.Modules.Session.Application.UseCases.CompleteSession;

namespace Popsies.Modules.Session.Tests.Unit.Validators;

public sealed class CompleteSessionCommandValidatorTests
{
    private readonly CompleteSessionCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new CompleteSessionCommand(SessionId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenSessionIdIsEmpty()
    {
        // Arrange
        var command = new CompleteSessionCommand(SessionId: Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "SessionId");
    }
}
