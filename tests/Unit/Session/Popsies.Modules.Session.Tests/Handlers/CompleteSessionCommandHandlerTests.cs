using FluentAssertions;
using NSubstitute;
using Popsies.Modules.Session.Application.Common.Repositories;
using Popsies.Modules.Session.Application.UseCases.CompleteSession;
using Popsies.Modules.Session.Domain.Sessions;
using Popsies.Shared.Abstractions.Persistence;

namespace Popsies.Modules.Session.Tests.Unit.Handlers;

public sealed class CompleteSessionCommandHandlerTests
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CompleteSessionCommandHandler _handler;

    public CompleteSessionCommandHandlerTests()
    {
        _sessionRepository = Substitute.For<ISessionRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new CompleteSessionCommandHandler(
            _sessionRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldCompleteSession_WhenValidCommand()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = SessionAggregate.Create(
            sessionId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ABC123",
            10,
            5).Value;

        session.AddPlayer(Guid.NewGuid(), Guid.NewGuid(), "Player1");
        session.Start();

        var command = new CompleteSessionCommand(sessionId);

        _sessionRepository.GetByIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.State.Should().Be(SessionState.Completed);
        _sessionRepository.Received(1).Update(session);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSessionNotFound()
    {
        // Arrange
        var command = new CompleteSessionCommand(Guid.NewGuid());

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
        var session = SessionAggregate.Create(
            sessionId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ABC123",
            10,
            5).Value;

        var command = new CompleteSessionCommand(sessionId);

        _sessionRepository.GetByIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Only active sessions can be completed");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDatabaseSaveFails()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = SessionAggregate.Create(
            sessionId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ABC123",
            10,
            5).Value;

        session.AddPlayer(Guid.NewGuid(), Guid.NewGuid(), "Player1");
        session.Start();

        var command = new CompleteSessionCommand(sessionId);

        _sessionRepository.GetByIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns(session);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Failed to complete session");
    }
}
