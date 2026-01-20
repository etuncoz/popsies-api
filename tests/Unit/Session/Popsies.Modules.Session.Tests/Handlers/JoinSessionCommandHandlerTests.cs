using FluentAssertions;
using NSubstitute;
using Popsies.Modules.Session.Application.Common.Repositories;
using Popsies.Modules.Session.Application.UseCases.JoinSession;
using Popsies.Shared.Abstractions.Persistence;

namespace Popsies.Modules.Session.Tests.Unit.Handlers;

public sealed class JoinSessionCommandHandlerTests
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JoinSessionCommandHandler _handler;

    public JoinSessionCommandHandlerTests()
    {
        _sessionRepository = Substitute.For<ISessionRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new JoinSessionCommandHandler(
            _sessionRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldAddPlayer_WhenValidCommand()
    {
        // Arrange
        var sessionCode = "ABC123";
        var userId = Guid.NewGuid();
        var displayName = "Player1";

        var session = SessionAggregate.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            sessionCode,
            10,
            5).Value;

        var command = new JoinSessionCommand(sessionCode, userId, displayName);

        _sessionRepository.GetBySessionCodeAsync(sessionCode, Arg.Any<CancellationToken>())
            .Returns(session);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        session.Players.Should().ContainSingle();
        _sessionRepository.Received(1).Update(session);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSessionNotFound()
    {
        // Arrange
        var command = new JoinSessionCommand("ABC123", Guid.NewGuid(), "Player1");

        _sessionRepository.GetBySessionCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((SessionAggregate?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Session.NotFound");
        _sessionRepository.DidNotReceive().Update(Arg.Any<SessionAggregate>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSessionIsFull()
    {
        // Arrange
        var session = SessionAggregate.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ABC123",
            2,
            5).Value;

        session.AddPlayer(Guid.NewGuid(), Guid.NewGuid(), "Player1");
        session.AddPlayer(Guid.NewGuid(), Guid.NewGuid(), "Player2");

        var command = new JoinSessionCommand("ABC123", Guid.NewGuid(), "Player3");

        _sessionRepository.GetBySessionCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(session);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Session has reached maximum player capacity");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDatabaseSaveFails()
    {
        // Arrange
        var session = SessionAggregate.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ABC123",
            10,
            5).Value;

        var command = new JoinSessionCommand("ABC123", Guid.NewGuid(), "Player1");

        _sessionRepository.GetBySessionCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(session);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Failed to join session");
    }
}
