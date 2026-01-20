using FluentAssertions;
using Popsies.Modules.Session.Domain.Sessions.Events;

namespace Popsies.Modules.Session.Tests.Unit.Domain.Aggregates;

public class SessionTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateSessionAndRaiseEvent()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var hostId = Guid.NewGuid();
        var sessionCode = "ABC123";
        var maxPlayers = 10;
        var totalQuestions = 5;

        // Act
        var result = SessionAggregate.Create(sessionId, quizId, hostId, sessionCode, maxPlayers, totalQuestions);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var session = result.Value;
        session.Id.Should().Be(sessionId);
        session.QuizId.Should().Be(quizId);
        session.HostId.Should().Be(hostId);
        session.SessionCode.Should().Be(sessionCode);
        session.MaxPlayers.Should().Be(maxPlayers);
        session.TotalQuestions.Should().Be(totalQuestions);
        session.State.Should().Be(SessionState.Waiting);
        session.CurrentQuestionIndex.Should().Be(0);
        session.Players.Should().BeEmpty();
        session.Answers.Should().BeEmpty();

        session.DomainEvents.Should().ContainSingle();
        session.DomainEvents.Should().ContainItemsAssignableTo<SessionCreatedEvent>();
    }

    [Fact]
    public void Create_WithEmptyQuizId_ShouldReturnFailure()
    {
        // Act
        var result = SessionAggregate.Create(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), "ABC123", 10, 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Quiz ID cannot be empty");
    }

    [Fact]
    public void Create_WithEmptyHostId_ShouldReturnFailure()
    {
        // Act
        var result = SessionAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, "ABC123", 10, 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Host ID cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptySessionCode_ShouldReturnFailure(string? sessionCode)
    {
        // Act
        var result = SessionAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), sessionCode!, 10, 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Session code cannot be empty");
    }

    [Theory]
    [InlineData("ABC12")] // Too short
    [InlineData("ABC1234")] // Too long
    public void Create_WithInvalidSessionCodeLength_ShouldReturnFailure(string sessionCode)
    {
        // Act
        var result = SessionAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), sessionCode, 10, 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("must be exactly 6 characters");
    }

    [Fact]
    public void Create_WithNonAlphanumericSessionCode_ShouldReturnFailure()
    {
        // Act
        var result = SessionAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "ABC-12", 10, 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("must contain only letters and numbers");
    }

    [Theory]
    [InlineData(1)] // Too few
    [InlineData(101)] // Too many
    public void Create_WithInvalidMaxPlayers_ShouldReturnFailure(int maxPlayers)
    {
        // Act
        var result = SessionAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "ABC123", maxPlayers, 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Max players must be between");
    }

    [Fact]
    public void Create_WithZeroTotalQuestions_ShouldReturnFailure()
    {
        // Act
        var result = SessionAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "ABC123", 10, 0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Total questions must be greater than zero");
    }

    [Fact]
    public void AddPlayer_WhenWaiting_ShouldAddPlayerAndRaiseEvent()
    {
        // Arrange
        var session = CreateTestSession();
        var playerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var displayName = "Player1";

        // Act
        var result = session.AddPlayer(playerId, userId, displayName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.Players.Should().ContainSingle();
        session.Players.First().Id.Should().Be(playerId);
        session.Players.First().DisplayName.Should().Be(displayName);
        session.DomainEvents.Should().Contain(e => e is PlayerJoinedEvent);
    }

    [Fact]
    public void AddPlayer_WhenNotWaiting_ShouldReturnFailure()
    {
        // Arrange
        var session = CreateTestSession();
        session.AddPlayer(Guid.NewGuid(), Guid.NewGuid(), "Player1");
        session.Start();

        // Act
        var result = session.AddPlayer(Guid.NewGuid(), Guid.NewGuid(), "Player2");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Players can only join sessions in waiting state");
    }

    [Fact]
    public void AddPlayer_WhenFull_ShouldReturnFailure()
    {
        // Arrange
        var session = SessionAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "ABC123", 2, 5).Value;
        session.AddPlayer(Guid.NewGuid(), Guid.NewGuid(), "Player1");
        session.AddPlayer(Guid.NewGuid(), Guid.NewGuid(), "Player2");

        // Act
        var result = session.AddPlayer(Guid.NewGuid(), Guid.NewGuid(), "Player3");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Session has reached maximum player capacity");
    }

    [Fact]
    public void AddPlayer_WithDuplicateUser_ShouldReturnFailure()
    {
        // Arrange
        var session = CreateTestSession();
        var userId = Guid.NewGuid();
        session.AddPlayer(Guid.NewGuid(), userId, "Player1");

        // Act
        var result = session.AddPlayer(Guid.NewGuid(), userId, "Player1Again");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("User has already joined this session");
    }

    [Fact]
    public void Start_WhenWaitingWithPlayers_ShouldStartAndRaiseEvent()
    {
        // Arrange
        var session = CreateTestSession();
        session.AddPlayer(Guid.NewGuid(), Guid.NewGuid(), "Player1");

        // Act
        var result = session.Start();

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.State.Should().Be(SessionState.Active);
        session.StartedAt.Should().NotBeNull();
        session.CurrentQuestionIndex.Should().Be(0);
        session.DomainEvents.Should().Contain(e => e is SessionStartedEvent);
    }

    [Fact]
    public void Start_WhenNotWaiting_ShouldReturnFailure()
    {
        // Arrange
        var session = CreateTestSession();
        session.AddPlayer(Guid.NewGuid(), Guid.NewGuid(), "Player1");
        session.Start();

        // Act
        var result = session.Start();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Only waiting sessions can be started");
    }

    [Fact]
    public void Start_WithoutPlayers_ShouldReturnFailure()
    {
        // Arrange
        var session = CreateTestSession();

        // Act
        var result = session.Start();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Session must have at least 1 active player");
    }

    [Fact]
    public void SubmitAnswer_WhenActive_ShouldAddAnswerAndUpdateScore()
    {
        // Arrange
        var session = CreateTestSession();
        var playerId = Guid.NewGuid();
        session.AddPlayer(playerId, Guid.NewGuid(), "Player1");
        session.Start();

        // Act
        var result = session.SubmitAnswer(
            Guid.NewGuid(),
            playerId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            true,
            100,
            10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.Answers.Should().ContainSingle();
        session.Players.First().TotalScore.Should().Be(100);
        session.Players.First().CorrectAnswers.Should().Be(1);
        session.DomainEvents.Should().Contain(e => e is AnswerSubmittedEvent);
    }

    [Fact]
    public void SubmitAnswer_WhenNotActive_ShouldReturnFailure()
    {
        // Arrange
        var session = CreateTestSession();
        var playerId = Guid.NewGuid();
        session.AddPlayer(playerId, Guid.NewGuid(), "Player1");

        // Act
        var result = session.SubmitAnswer(Guid.NewGuid(), playerId, Guid.NewGuid(), Guid.NewGuid(), true, 100, 10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Answers can only be submitted to active sessions");
    }

    [Fact]
    public void SubmitAnswer_WithDuplicateAnswer_ShouldReturnFailure()
    {
        // Arrange
        var session = CreateTestSession();
        var playerId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        session.AddPlayer(playerId, Guid.NewGuid(), "Player1");
        session.Start();
        session.SubmitAnswer(Guid.NewGuid(), playerId, questionId, Guid.NewGuid(), true, 100, 10);

        // Act
        var result = session.SubmitAnswer(Guid.NewGuid(), playerId, questionId, Guid.NewGuid(), false, 0, 15);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Player has already answered this question");
    }

    [Fact]
    public void AdvanceToNextQuestion_WhenActive_ShouldIncreaseIndex()
    {
        // Arrange
        var session = CreateTestSession();
        session.AddPlayer(Guid.NewGuid(), Guid.NewGuid(), "Player1");
        session.Start();

        // Act
        var result = session.AdvanceToNextQuestion();

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.CurrentQuestionIndex.Should().Be(1);
    }

    [Fact]
    public void AdvanceToNextQuestion_WhenAtLastQuestion_ShouldReturnFailure()
    {
        // Arrange
        var session = SessionAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "ABC123", 10, 2).Value;
        session.AddPlayer(Guid.NewGuid(), Guid.NewGuid(), "Player1");
        session.Start();
        session.AdvanceToNextQuestion();

        // Act
        var result = session.AdvanceToNextQuestion();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("No more questions available");
    }

    [Fact]
    public void Complete_WhenActive_ShouldCompleteAndRaiseEvent()
    {
        // Arrange
        var session = CreateTestSession();
        session.AddPlayer(Guid.NewGuid(), Guid.NewGuid(), "Player1");
        session.Start();

        // Act
        var result = session.Complete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.State.Should().Be(SessionState.Completed);
        session.CompletedAt.Should().NotBeNull();
        session.DomainEvents.Should().Contain(e => e is SessionCompletedEvent);
    }

    [Fact]
    public void Complete_WhenNotActive_ShouldReturnFailure()
    {
        // Arrange
        var session = CreateTestSession();

        // Act
        var result = session.Complete();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Only active sessions can be completed");
    }

    [Fact]
    public void Cancel_WhenWaiting_ShouldCancel()
    {
        // Arrange
        var session = CreateTestSession();

        // Act
        var result = session.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.State.Should().Be(SessionState.Cancelled);
    }

    [Fact]
    public void Cancel_WhenCompleted_ShouldReturnFailure()
    {
        // Arrange
        var session = CreateTestSession();
        session.AddPlayer(Guid.NewGuid(), Guid.NewGuid(), "Player1");
        session.Start();
        session.Complete();

        // Act
        var result = session.Cancel();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Cannot cancel completed session");
    }

    [Fact]
    public void GetLeaderboard_ShouldReturnPlayersSortedByScore()
    {
        // Arrange
        var session = CreateTestSession();
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var player3Id = Guid.NewGuid();

        session.AddPlayer(player1Id, Guid.NewGuid(), "Player1");
        session.AddPlayer(player2Id, Guid.NewGuid(), "Player2");
        session.AddPlayer(player3Id, Guid.NewGuid(), "Player3");
        session.Start();

        session.SubmitAnswer(Guid.NewGuid(), player1Id, Guid.NewGuid(), Guid.NewGuid(), true, 50, 10);
        session.SubmitAnswer(Guid.NewGuid(), player2Id, Guid.NewGuid(), Guid.NewGuid(), true, 100, 5);
        session.SubmitAnswer(Guid.NewGuid(), player3Id, Guid.NewGuid(), Guid.NewGuid(), true, 75, 8);

        // Act
        var leaderboard = session.GetLeaderboard();

        // Assert
        leaderboard.Should().HaveCount(3);
        leaderboard[0].Id.Should().Be(player2Id);
        leaderboard[1].Id.Should().Be(player3Id);
        leaderboard[2].Id.Should().Be(player1Id);
    }

    private static SessionAggregate CreateTestSession()
    {
        var result = SessionAggregate.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ABC123",
            10,
            5);

        return result.Value;
    }
}
