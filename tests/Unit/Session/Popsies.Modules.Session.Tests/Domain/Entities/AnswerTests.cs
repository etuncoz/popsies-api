using FluentAssertions;
using Popsies.Modules.Session.Domain.Answers;

namespace Popsies.Modules.Session.Tests.Unit.Domain.Entities;

public class AnswerTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateAnswer()
    {
        // Arrange
        var answerId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var selectedItemId = Guid.NewGuid();

        // Act
        var result = Answer.Create(answerId, sessionId, playerId, questionId, selectedItemId, true, 100, 15);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var answer = result.Value;
        answer.Id.Should().Be(answerId);
        answer.SessionId.Should().Be(sessionId);
        answer.PlayerId.Should().Be(playerId);
        answer.QuestionId.Should().Be(questionId);
        answer.SelectedItemId.Should().Be(selectedItemId);
        answer.IsCorrect.Should().BeTrue();
        answer.PointsEarned.Should().Be(100);
        answer.TimeTakenSeconds.Should().Be(15);
        answer.SubmittedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithEmptySessionId_ShouldReturnFailure()
    {
        // Act
        var result = Answer.Create(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), true, 100, 10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Session ID cannot be empty");
    }

    [Fact]
    public void Create_WithEmptyPlayerId_ShouldReturnFailure()
    {
        // Act
        var result = Answer.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), true, 100, 10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Player ID cannot be empty");
    }

    [Fact]
    public void Create_WithEmptyQuestionId_ShouldReturnFailure()
    {
        // Act
        var result = Answer.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), true, 100, 10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Question ID cannot be empty");
    }

    [Fact]
    public void Create_WithEmptySelectedItemId_ShouldReturnFailure()
    {
        // Act
        var result = Answer.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, true, 100, 10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Selected item ID cannot be empty");
    }

    [Fact]
    public void Create_WithNegativePointsEarned_ShouldReturnFailure()
    {
        // Act
        var result = Answer.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), true, -50, 10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Points earned cannot be negative");
    }

    [Fact]
    public void Create_WithNegativeTimeTaken_ShouldReturnFailure()
    {
        // Act
        var result = Answer.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), true, 100, -5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Time taken cannot be negative");
    }
}
