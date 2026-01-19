using FluentAssertions;
using NSubstitute;
using Popsies.Modules.Identity.Core.Commands;
using Popsies.Modules.Identity.Core.Domain.Aggregates;
using Popsies.Modules.Identity.Core.Domain.Entities;
using Popsies.Modules.Identity.Core.Domain.Enums;
using Popsies.Modules.Identity.Core.Domain.ValueObjects;
using Popsies.Modules.Identity.Core.Handlers;
using Popsies.Modules.Identity.Core.Repositories;
using Popsies.Modules.Identity.Core.Services;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;
using Popsies.Shared.Abstractions.Time;

namespace Popsies.Modules.Identity.Tests.Handlers;

public sealed class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IKeycloakService _keycloakService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTime _dateTime;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _accountRepository = Substitute.For<IAccountRepository>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _keycloakService = Substitute.For<IKeycloakService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _dateTime = Substitute.For<IDateTime>();

        _handler = new LoginCommandHandler(
            _userRepository,
            _accountRepository,
            _refreshTokenRepository,
            _keycloakService,
            _unitOfWork,
            _dateTime);
    }

    [Fact]
    public async Task Handle_ShouldReturnLoginResult_WhenCredentialsAreValid()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var username = Username.Create("testuser", 1234).Value;

        var userId = Guid.NewGuid();
        var user = User.Create(userId, username, email);
        user.LinkToKeycloak("keycloak-user-123");
        var accountId = Guid.NewGuid();
        var account = Account.Create(accountId, userId, username, email);
        account.VerifyEmail();

        var command = new LoginCommand(
            UsernameOrEmail: "test@example.com",
            Password: "Password123!",
            DeviceInfo: "Test Device");

        var accessToken = "access_token";
        var refreshToken = "refresh_token";
        var currentTime = DateTime.UtcNow;

        _keycloakService.AuthenticateAsync("test@example.com", "Password123!", Arg.Any<CancellationToken>())
            .Returns(new KeycloakAuthResult(accessToken, refreshToken, 3600, "Bearer"));
        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(user);
        _accountRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(account);
        _dateTime.UtcNow.Returns(currentTime);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        result.Value.AccessToken.Should().Be(accessToken);
        result.Value.RefreshToken.Should().Be(refreshToken);
        await _keycloakService.Received(1).AuthenticateAsync("test@example.com", "Password123!", Arg.Any<CancellationToken>());
        await _refreshTokenRepository.Received(1).AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var command = new LoginCommand(
            UsernameOrEmail: "nonexistent@example.com",
            Password: "Password123!",
            DeviceInfo: "Test Device");

        var email = Email.Create("nonexistent@example.com").Value;
        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Invalid username/email or password");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPasswordIsInvalid()
    {
        // Arrange
        var command = new LoginCommand(
            UsernameOrEmail: "test@example.com",
            Password: "WrongPassword123!",
            DeviceInfo: "Test Device");

        _keycloakService.AuthenticateAsync("test@example.com", "WrongPassword123!", Arg.Any<CancellationToken>())
            .Returns<KeycloakAuthResult>(_ => throw new UnauthorizedAccessException("Invalid credentials"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Invalid username/email or password");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAccountIsSuspended()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var username = Username.Create("testuser", 1234).Value;

        var userId = Guid.NewGuid();
        var user = User.Create(userId, username, email);
        user.LinkToKeycloak("keycloak-user-123");
        var accountId = Guid.NewGuid();
        var account = Account.Create(accountId, userId, username, email);
        account.Suspend();

        var command = new LoginCommand(
            UsernameOrEmail: "test@example.com",
            Password: "Password123!",
            DeviceInfo: "Test Device");

        var accessToken = "access_token";
        var refreshToken = "refresh_token";

        _keycloakService.AuthenticateAsync("test@example.com", "Password123!", Arg.Any<CancellationToken>())
            .Returns(new KeycloakAuthResult(accessToken, refreshToken, 3600, "Bearer"));
        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(user);
        _accountRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(account);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("suspended");
    }

    [Fact]
    public async Task Handle_ShouldSupportLoginByUsername()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var username = Username.Create("testuser", 1234).Value;

        var userId = Guid.NewGuid();
        var user = User.Create(userId, username, email);
        user.LinkToKeycloak("keycloak-user-123");
        var accountId = Guid.NewGuid();
        var account = Account.Create(accountId, userId, username, email);
        account.VerifyEmail();

        var command = new LoginCommand(
            UsernameOrEmail: "testuser#1234",
            Password: "Password123!",
            DeviceInfo: "Test Device");

        var accessToken = "access_token";
        var refreshToken = "refresh_token";
        var currentTime = DateTime.UtcNow;

        _keycloakService.AuthenticateAsync("testuser#1234", "Password123!", Arg.Any<CancellationToken>())
            .Returns(new KeycloakAuthResult(accessToken, refreshToken, 3600, "Bearer"));
        _userRepository.GetByUsernameAsync(username, Arg.Any<CancellationToken>())
            .Returns(user);
        _accountRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(account);
        _dateTime.UtcNow.Returns(currentTime);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
    }
}
