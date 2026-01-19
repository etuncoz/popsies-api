using FluentAssertions;
using NSubstitute;
using Popsies.Modules.Identity.Core.Commands;
using Popsies.Modules.Identity.Core.Domain.Entities;
using Popsies.Modules.Identity.Core.Handlers;
using Popsies.Modules.Identity.Core.Repositories;
using Popsies.Modules.Identity.Core.Services;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;
using Popsies.Shared.Abstractions.Time;

namespace Popsies.Modules.Identity.Tests.Handlers;

public sealed class CreateGuestCommandHandlerTests
{
    private readonly IGuestRepository _guestRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IGuestTokenGenerator _guestTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTime _dateTime;
    private readonly CreateGuestCommandHandler _handler;

    public CreateGuestCommandHandlerTests()
    {
        _guestRepository = Substitute.For<IGuestRepository>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _guestTokenGenerator = Substitute.For<IGuestTokenGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _dateTime = Substitute.For<IDateTime>();

        _handler = new CreateGuestCommandHandler(
            _guestRepository,
            _refreshTokenRepository,
            _guestTokenGenerator,
            _unitOfWork,
            _dateTime);
    }

    [Fact]
    public async Task Handle_ShouldCreateGuestAndReturnTokens_WhenValidCommand()
    {
        // Arrange
        var command = new CreateGuestCommand(
            DisplayName: "GuestUser",
            DeviceInfo: "Test Device");

        var accessToken = "access_token";
        var refreshToken = "refresh_token";
        var currentTime = DateTime.UtcNow;

        _guestTokenGenerator.GenerateAccessToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>())
            .Returns(accessToken);
        _guestTokenGenerator.GenerateRefreshToken()
            .Returns(refreshToken);
        _guestTokenGenerator.GetAccessTokenExpirationMinutes()
            .Returns(60);
        _dateTime.UtcNow.Returns(currentTime);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.GuestId.Should().NotBe(Guid.Empty);
        result.Value.AccessToken.Should().Be(accessToken);
        result.Value.RefreshToken.Should().Be(refreshToken);
        result.Value.ExpiresAt.Should().Be(currentTime.AddMinutes(60));
        await _guestRepository.Received(1).AddAsync(Arg.Any<Guest>(), Arg.Any<CancellationToken>());
        await _refreshTokenRepository.Received(1).AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDisplayNameIsEmpty()
    {
        // Arrange
        var command = new CreateGuestCommand(
            DisplayName: "",
            DeviceInfo: "Test Device");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Display name cannot be empty");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDisplayNameIsTooLong()
    {
        // Arrange
        var command = new CreateGuestCommand(
            DisplayName: new string('a', 21), // Max is 20
            DeviceInfo: "Test Device");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Display name must be");
        result.Error.Message.Should().Contain("20 characters");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDisplayNameIsTooShort()
    {
        // Arrange
        var command = new CreateGuestCommand(
            DisplayName: "ab", // Min is 3
            DeviceInfo: "Test Device");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Display name must be");
        result.Error.Message.Should().Contain("3");
        result.Error.Message.Should().Contain("20 characters");
    }

    [Fact]
    public async Task Handle_ShouldCreateGuestWith24HourExpiration()
    {
        // Arrange
        var command = new CreateGuestCommand(
            DisplayName: "GuestUser",
            DeviceInfo: "Test Device");

        var currentTime = DateTime.UtcNow;
        _dateTime.UtcNow.Returns(currentTime);
        _guestTokenGenerator.GenerateAccessToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>())
            .Returns("access_token");
        _guestTokenGenerator.GenerateRefreshToken()
            .Returns("refresh_token");
        _guestTokenGenerator.GetAccessTokenExpirationMinutes()
            .Returns(60);

        Guest? capturedGuest = null;
        await _guestRepository.AddAsync(Arg.Do<Guest>(g => capturedGuest = g), Arg.Any<CancellationToken>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedGuest.Should().NotBeNull();
        capturedGuest!.ExpiresAt.Should().BeCloseTo(currentTime.AddHours(24), TimeSpan.FromSeconds(1));
        capturedGuest.IsExpired.Should().BeFalse();
    }
}
