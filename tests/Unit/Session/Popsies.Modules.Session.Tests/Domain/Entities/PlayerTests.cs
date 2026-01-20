using FluentAssertions;
using Popsies.Modules.Session.Domain.Players;

namespace Popsies.Modules.Session.Tests.Unit.Domain.Entities;

public class PlayerTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreatePlayer()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var displayName = "TestPlayer";

        // Act
        var result = Player.Create(playerId, sessionId, userId, displayName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var player = result.Value;
        player.Id.Should().Be(playerId);
        player.SessionId.Should().Be(sessionId);
        player.UserId.Should().Be(userId);
        player.DisplayName.Should().Be(displayName);
        player.TotalScore.Should().Be(0);
        player.CorrectAnswers.Should().Be(0);
        player.IsActive.Should().BeTrue();
        player.LeftAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptySessionId_ShouldReturnFailure()
    {
        // Act
        var result = Player.Create(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), "Player");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Session ID cannot be empty");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldReturnFailure()
    {
        // Act
        var result = Player.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, "Player");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("User ID cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyDisplayName_ShouldReturnFailure(string? displayName)
    {
        // Act
        var result = Player.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), displayName!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Display name cannot be empty");
    }

    [Fact]
    public void Create_WithTooLongDisplayName_ShouldReturnFailure()
    {
        // Arrange
        var displayName = new string('A', 51);

        // Act
        var result = Player.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), displayName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("must be 1-50 characters long");
    }

    [Fact]
    public void AddScore_WithValidPoints_ShouldIncreaseScore()
    {
        // Arrange
        var player = Player.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Player").Value;

        // Act
        var result = player.AddScore(100, true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        player.TotalScore.Should().Be(100);
        player.CorrectAnswers.Should().Be(1);
    }

    [Fact]
    public void AddScore_WithIncorrectAnswer_ShouldNotIncreaseCorrectAnswers()
    {
        // Arrange
        var player = Player.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Player").Value;

        // Act
        var result = player.AddScore(0, false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        player.TotalScore.Should().Be(0);
        player.CorrectAnswers.Should().Be(0);
    }

    [Fact]
    public void AddScore_WhenInactive_ShouldReturnFailure()
    {
        // Arrange
        var player = Player.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Player").Value;
        player.Leave();

        // Act
        var result = player.AddScore(100, true);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Cannot add score to inactive player");
    }

    [Fact]
    public void AddScore_WithNegativePoints_ShouldReturnFailure()
    {
        // Arrange
        var player = Player.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Player").Value;

        // Act
        var result = player.AddScore(-50, true);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Points cannot be negative");
    }

    [Fact]
    public void Leave_WhenActive_ShouldMarkAsInactive()
    {
        // Arrange
        var player = Player.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Player").Value;

        // Act
        var result = player.Leave();

        // Assert
        result.IsSuccess.Should().BeTrue();
        player.IsActive.Should().BeFalse();
        player.LeftAt.Should().NotBeNull();
    }

    [Fact]
    public void Leave_WhenAlreadyLeft_ShouldReturnFailure()
    {
        // Arrange
        var player = Player.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Player").Value;
        player.Leave();

        // Act
        var result = player.Leave();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Player has already left");
    }
}
