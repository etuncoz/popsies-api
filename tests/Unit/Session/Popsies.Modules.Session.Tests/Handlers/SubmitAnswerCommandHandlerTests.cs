using FluentAssertions;
using NSubstitute;
using Popsies.Modules.Session.Application.Common.Repositories;
using Popsies.Modules.Session.Application.UseCases.SubmitAnswer;
using Popsies.Shared.Abstractions.Persistence;

namespace Popsies.Modules.Session.Tests.Unit.Handlers;

public sealed class SubmitAnswerCommandHandlerTests
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SubmitAnswerCommandHandler _handler;

    public SubmitAnswerCommandHandlerTests()
    {
        _sessionRepository = Substitute.For<ISessionRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new SubmitAnswerCommandHandler(
            _sessionRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldSubmitAnswer_WhenValidCommand()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var selectedItemId = Guid.NewGuid();

        var session = SessionAggregate.Create(
            sessionId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ABC123",
            10,
            5).Value;

        session.AddPlayer(playerId, Guid.NewGuid(), "Player1");
        session.Start();

        var command = new SubmitAnswerCommand(sessionId, playerId, questionId, selectedItemId, 10);

        _sessionRepository.GetByIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        session.Answers.Should().ContainSingle();
        _sessionRepository.Received(1).Update(session);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSessionNotFound()
    {
        // Arrange
        var command = new SubmitAnswerCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            10);

        _sessionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((SessionAggregate?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Session.NotFound");
        _sessionRepository.DidNotReceive().Update(Arg.Any<SessionAggregate>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSessionNotActive()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        var session = SessionAggregate.Create(
            sessionId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ABC123",
            10,
            5).Value;

        session.AddPlayer(playerId, Guid.NewGuid(), "Player1");

        var command = new SubmitAnswerCommand(
            sessionId,
            playerId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            10);

        _sessionRepository.GetByIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Answers can only be submitted to active sessions");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDatabaseSaveFails()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        var session = SessionAggregate.Create(
            sessionId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ABC123",
            10,
            5).Value;

        session.AddPlayer(playerId, Guid.NewGuid(), "Player1");
        session.Start();

        var command = new SubmitAnswerCommand(
            sessionId,
            playerId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            10);

        _sessionRepository.GetByIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns(session);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Failed to submit answer");
    }
}
