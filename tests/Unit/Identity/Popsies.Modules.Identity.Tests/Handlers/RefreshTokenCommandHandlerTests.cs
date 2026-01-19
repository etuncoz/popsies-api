using FluentAssertions;
using NSubstitute;
using Popsies.Modules.Identity.Core.Commands;
using Popsies.Modules.Identity.Core.Domain.Aggregates;
using Popsies.Modules.Identity.Core.Domain.Entities;
using Popsies.Modules.Identity.Core.Domain.ValueObjects;
using Popsies.Modules.Identity.Core.Handlers;
using Popsies.Modules.Identity.Core.Repositories;
using Popsies.Modules.Identity.Core.Services;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;
using Popsies.Shared.Abstractions.Time;

namespace Popsies.Modules.Identity.Tests.Handlers;

public sealed class RefreshTokenCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IKeycloakService _keycloakService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTime _dateTime;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _keycloakService = Substitute.For<IKeycloakService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _dateTime = Substitute.For<IDateTime>();

        _handler = new RefreshTokenCommandHandler(
            _userRepository,
            _refreshTokenRepository,
            _keycloakService,
            _unitOfWork,
            _dateTime);
    }

    [Fact]
    public async Task Handle_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var username = Username.Create("testuser", 1234).Value;
        var userId = Guid.NewGuid();
        var user = User.Create(userId, username, email);
        user.LinkToKeycloak("keycloak-user-123");

        var oldRefreshTokenString = "old_refresh_token";
        var tokenId = Guid.NewGuid();
        var existingToken = RefreshToken.Create(tokenId, userId, oldRefreshTokenString, "Device 1").Value;

        var command = new RefreshTokenCommand(
            RefreshToken: oldRefreshTokenString,
            DeviceInfo: "Device 1");

        var newAccessToken = "new_access_token";
        var newRefreshTokenString = "new_refresh_token";
        var currentTime = DateTime.UtcNow;

        _refreshTokenRepository.GetByTokenAsync(oldRefreshTokenString, Arg.Any<CancellationToken>())
            .Returns(existingToken);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _keycloakService.RefreshTokenAsync(oldRefreshTokenString, Arg.Any<CancellationToken>())
            .Returns(new KeycloakTokenResult(newAccessToken, newRefreshTokenString, 3600));
        _dateTime.UtcNow.Returns(currentTime);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be(newAccessToken);
        result.Value.RefreshToken.Should().Be(newRefreshTokenString);
        existingToken.IsRevoked.Should().BeTrue();
        await _keycloakService.Received(1).RefreshTokenAsync(oldRefreshTokenString, Arg.Any<CancellationToken>());
        await _refreshTokenRepository.Received(1).AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTokenNotFound()
    {
        // Arrange
        var command = new RefreshTokenCommand(
            RefreshToken: "invalid_token",
            DeviceInfo: "Device 1");

        _refreshTokenRepository.GetByTokenAsync("invalid_token", Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Invalid refresh token");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTokenIsRevoked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenString = "revoked_token";
        var tokenId = Guid.NewGuid();
        var refreshToken = RefreshToken.Create(tokenId, userId, tokenString, "Device 1").Value;
        refreshToken.Revoke();

        var command = new RefreshTokenCommand(
            RefreshToken: tokenString,
            DeviceInfo: "Device 1");

        _refreshTokenRepository.GetByTokenAsync(tokenString, Arg.Any<CancellationToken>())
            .Returns(refreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Refresh token has been revoked");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTokenIsExpired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenString = "expired_token";
        var tokenId = Guid.NewGuid();
        var refreshToken = RefreshToken.Create(tokenId, userId, tokenString, "Device 1").Value;

        // Mark as expired
        var currentTime = DateTime.UtcNow.AddDays(8); // 8 days later (token expires in 7 days)
        refreshToken.CheckExpiration(currentTime);

        var command = new RefreshTokenCommand(
            RefreshToken: tokenString,
            DeviceInfo: "Device 1");

        _refreshTokenRepository.GetByTokenAsync(tokenString, Arg.Any<CancellationToken>())
            .Returns(refreshToken);
        _dateTime.UtcNow.Returns(currentTime);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Refresh token has expired");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenString = "valid_token";
        var tokenId = Guid.NewGuid();
        var refreshToken = RefreshToken.Create(tokenId, userId, tokenString, "Device 1").Value;

        var command = new RefreshTokenCommand(
            RefreshToken: tokenString,
            DeviceInfo: "Device 1");

        _refreshTokenRepository.GetByTokenAsync(tokenString, Arg.Any<CancellationToken>())
            .Returns(refreshToken);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("User");
        result.Error.Message.Should().Contain("not found");
    }
}
