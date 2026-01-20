using FluentAssertions;
using Popsies.Modules.Session.Application.UseCases.StartSession;

namespace Popsies.Modules.Session.Tests.Unit.Validators;

public sealed class StartSessionCommandValidatorTests
{
    private readonly StartSessionCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new StartSessionCommand(SessionId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenSessionIdIsEmpty()
    {
        // Arrange
        var command = new StartSessionCommand(SessionId: Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "SessionId");
    }
}
