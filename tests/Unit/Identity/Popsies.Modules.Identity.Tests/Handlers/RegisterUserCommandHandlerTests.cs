using FluentAssertions;
using NSubstitute;
using Popsies.Modules.Identity.Application.UseCases.Register;
using Popsies.Modules.Identity.Application.UseCases.Login;
using Popsies.Modules.Identity.Application.UseCases.RefreshToken;
using Popsies.Modules.Identity.Application.UseCases.CreateGuest;
using Popsies.Modules.Identity.Domain.Users;
using Popsies.Modules.Identity.Domain.Accounts;
using Popsies.Modules.Identity.Domain.Guests;
using Popsies.Modules.Identity.Domain.RefreshTokens;
using Popsies.Modules.Identity.Domain.Accounts;
using Popsies.Modules.Identity.Domain.ValueObjects;
using Popsies.Modules.Identity.Application.UseCases.Register;
using Popsies.Modules.Identity.Application.UseCases.Login;
using Popsies.Modules.Identity.Application.UseCases.RefreshToken;
using Popsies.Modules.Identity.Application.UseCases.CreateGuest;
using Popsies.Modules.Identity.Application.Common.Repositories;
using Popsies.Modules.Identity.Application.Common.Services;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;
using Popsies.Shared.Abstractions.Time;

namespace Popsies.Modules.Identity.Tests.Handlers;

public sealed class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IKeycloakService _keycloakService;
    private readonly IUsernameGenerator _usernameGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTime _dateTime;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _accountRepository = Substitute.For<IAccountRepository>();
        _keycloakService = Substitute.For<IKeycloakService>();
        _usernameGenerator = Substitute.For<IUsernameGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _dateTime = Substitute.For<IDateTime>();

        _handler = new RegisterUserCommandHandler(
            _userRepository,
            _accountRepository,
            _keycloakService,
            _usernameGenerator,
            _unitOfWork,
            _dateTime);
    }

    [Fact]
    public async Task Handle_ShouldCreateUserAndAccount_WhenValidCommand()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Username: "testuser",
            Email: "test@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!");

        var username = Username.Create("testuser", 1234).Value;
        var keycloakUserId = "keycloak-user-123";
        var currentTime = DateTime.UtcNow;

        _usernameGenerator.GenerateUniqueUsernameAsync(command.Username, Arg.Any<CancellationToken>())
            .Returns(Result.Success(username));
        _userRepository.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _keycloakService.CreateUserAsync(
            username.DisplayName,
            username.Discriminator,
            command.Email,
            command.Password,
            Arg.Any<CancellationToken>())
            .Returns(new KeycloakUserCreationResult(keycloakUserId, true));
        _dateTime.UtcNow.Returns(currentTime);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _keycloakService.Received(1).CreateUserAsync(
            username.DisplayName,
            username.Discriminator,
            command.Email,
            command.Password,
            Arg.Any<CancellationToken>());
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _accountRepository.Received(1).AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPasswordsDoNotMatch()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Username: "testuser",
            Email: "test@example.com",
            Password: "Password123!",
            ConfirmPassword: "DifferentPassword123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Passwords do not match");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Username: "testuser",
            Email: "test@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!");

        var username = Username.Create("testuser", 1234).Value;
        _usernameGenerator.GenerateUniqueUsernameAsync(command.Username, Arg.Any<CancellationToken>())
            .Returns(Result.Success(username));
        _userRepository.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Email is already in use");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailIsInvalid()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Username: "testuser",
            Email: "invalid-email",
            Password: "Password123!",
            ConfirmPassword: "Password123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Invalid email format");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPasswordIsTooWeak()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Username: "testuser",
            Email: "test@example.com",
            Password: "weak",
            ConfirmPassword: "weak");

        var username = Username.Create("testuser", 1234).Value;
        _usernameGenerator.GenerateUniqueUsernameAsync(command.Username, Arg.Any<CancellationToken>())
            .Returns(Result.Success(username));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUsernameIsInvalid()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Username: "ab",  // Too short
            Email: "test@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!");

        _usernameGenerator.GenerateUniqueUsernameAsync(command.Username, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Username>(Error.Validation("Username", "Display name must be between 3 and 20 characters")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Display name must be between 3 and 20 characters");
    }

    [Fact]
    public async Task Handle_ShouldCreateAccountInPendingState()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Username: "testuser",
            Email: "test@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!");

        var username = Username.Create("testuser", 1234).Value;
        var keycloakUserId = "keycloak-user-123";

        _usernameGenerator.GenerateUniqueUsernameAsync(command.Username, Arg.Any<CancellationToken>())
            .Returns(Result.Success(username));
        _userRepository.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _keycloakService.CreateUserAsync(
            username.DisplayName,
            username.Discriminator,
            command.Email,
            command.Password,
            Arg.Any<CancellationToken>())
            .Returns(new KeycloakUserCreationResult(keycloakUserId, true));
        _dateTime.UtcNow.Returns(DateTime.UtcNow);

        Account? capturedAccount = null;
        await _accountRepository.AddAsync(Arg.Do<Account>(a => capturedAccount = a), Arg.Any<CancellationToken>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedAccount.Should().NotBeNull();
        capturedAccount!.State.Should().Be(AccountState.Pending);
        capturedAccount.IsEmailVerified.Should().BeFalse();
    }
}
