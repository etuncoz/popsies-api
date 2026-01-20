using FluentAssertions;
using NSubstitute;
using Popsies.Modules.Session.Application.Common.Repositories;
using Popsies.Modules.Session.Application.UseCases.CreateSession;
using Popsies.Shared.Abstractions.Persistence;

namespace Popsies.Modules.Session.Tests.Unit.Handlers;

public sealed class CreateSessionCommandHandlerTests
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateSessionCommandHandler _handler;

    public CreateSessionCommandHandlerTests()
    {
        _sessionRepository = Substitute.For<ISessionRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new CreateSessionCommandHandler(
            _sessionRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldCreateSession_WhenValidCommand()
    {
        // Arrange
        var command = new CreateSessionCommand(
            QuizId: Guid.NewGuid(),
            HostId: Guid.NewGuid(),
            MaxPlayers: 10);

        _sessionRepository.ExistsBySessionCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _sessionRepository.Received(1).AddAsync(Arg.Any<SessionAggregate>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldGenerateUniqueSessionCode()
    {
        // Arrange
        var command = new CreateSessionCommand(
            QuizId: Guid.NewGuid(),
            HostId: Guid.NewGuid(),
            MaxPlayers: 10);

        _sessionRepository.ExistsBySessionCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true, true, false); // First two codes exist, third is unique

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _sessionRepository.Received(3).ExistsBySessionCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDatabaseSaveFails()
    {
        // Arrange
        var command = new CreateSessionCommand(
            QuizId: Guid.NewGuid(),
            HostId: Guid.NewGuid(),
            MaxPlayers: 10);

        _sessionRepository.ExistsBySessionCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Failed to create session");
    }
}
