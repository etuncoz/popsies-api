using Popsies.Modules.Identity.Core.Commands;
using Popsies.Modules.Identity.Core.Domain.Aggregates;
using Popsies.Modules.Identity.Core.Domain.Entities;
using Popsies.Modules.Identity.Core.Domain.ValueObjects;
using Popsies.Modules.Identity.Core.Repositories;
using Popsies.Modules.Identity.Core.Services;
using Popsies.Shared.Abstractions.Commands;
using Popsies.Shared.Abstractions.Exceptions;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;
using Popsies.Shared.Abstractions.Time;

namespace Popsies.Modules.Identity.Core.Handlers;

internal sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IAccountRepository accountRepository,
    IKeycloakService keycloakService,
    IUsernameGenerator usernameGenerator,
    IUnitOfWork unitOfWork,
    IDateTime dateTime) : ICommandHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IUsernameGenerator _usernameGenerator = usernameGenerator;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IDateTime _dateTime = dateTime;

    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        // Validate password confirmation
        if (command.Password != command.ConfirmPassword)
        {
            return Result.Failure<Guid>(Error.Validation("Password", "Passwords do not match"));
        }

        // Create value objects and validate
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<Guid>(emailResult.Error);
        }

        var usernameResult = await _usernameGenerator.GenerateUniqueUsernameAsync(command.Username, cancellationToken);
        if (usernameResult.IsFailure)
        {
            return Result.Failure<Guid>(usernameResult.Error);
        }

        var username = usernameResult.Value;

        // Validate password (without storing hash)
        var passwordResult = Password.Create(command.Password, command.Username);
        if (passwordResult.IsFailure)
        {
            return Result.Failure<Guid>(passwordResult.Error);
        }

        var email = emailResult.Value;

        // Check if email already exists
        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            return Result.Failure<Guid>(Error.Conflict("Email is already in use"));
        }

        // Create user in Keycloak with custom attributes
        var keycloakResult = await _keycloakService.CreateUserAsync(
            username.DisplayName,
            username.Discriminator,
            email.Value,
            command.Password,
            cancellationToken);

        if (!keycloakResult.Success)
        {
            return Result.Failure<Guid>(
                Error.Create("Keycloak.UserCreation", keycloakResult.ErrorMessage ?? "Failed to create user in Keycloak"));
        }

        // Create user aggregate
        var userId = Guid.NewGuid();
        var user = User.Create(userId, username, email);

        // Link to Keycloak
        var linkResult = user.LinkToKeycloak(keycloakResult.KeycloakUserId!);
        if (linkResult.IsFailure)
        {
            return Result.Failure<Guid>(linkResult.Error);
        }

        // Create account entity (password managed by Keycloak)
        var accountId = Guid.NewGuid();
        var account = Account.Create(
            accountId,
            userId,
            username,
            email);

        // Persist to local database
        await _userRepository.AddAsync(user, cancellationToken);
        await _accountRepository.AddAsync(account, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(Error.Create("Database.SaveFailed", $"Failed to save user: {ex.Message}"));
        }

        return Result<Guid>.Success(userId);
    }
}
