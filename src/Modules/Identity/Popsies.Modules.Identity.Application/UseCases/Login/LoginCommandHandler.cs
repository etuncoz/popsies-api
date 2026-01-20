using MediatR;
using Popsies.Modules.Identity.Application.Common.Repositories;
using Popsies.Modules.Identity.Application.Common.Services;
using Popsies.Modules.Identity.Domain.Accounts;
using Popsies.Modules.Identity.Domain.Users;
using Popsies.Modules.Identity.Domain.ValueObjects;
using Popsies.Shared.Abstractions.Exceptions;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;
using Popsies.Shared.Abstractions.Time;
using RefreshTokenEntity = Popsies.Modules.Identity.Domain.RefreshTokens.RefreshToken;

namespace Popsies.Modules.Identity.Application.UseCases.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IAccountRepository accountRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IKeycloakService keycloakService,
    IUnitOfWork unitOfWork,
    IDateTime dateTime) : IRequestHandler<LoginCommand, Result<LoginResult>>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IDateTime _dateTime = dateTime;

    public async Task<Result<LoginResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Authenticate via Keycloak (password grant)
        KeycloakAuthResult keycloakAuth;
        try
        {
            keycloakAuth = await _keycloakService.AuthenticateAsync(
                request.UsernameOrEmail,
                request.Password,
                cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            return Result.Failure<LoginResult>(Error.Unauthorized("Invalid username/email or password"));
        }
        catch (Exception ex)
        {
            return Result.Failure<LoginResult>(Error.Create("Authentication.Failed", $"Authentication failed: {ex.Message}"));
        }

        // Determine if login is by email or username
        User? user;
        if (request.UsernameOrEmail.Contains('@'))
        {
            var emailResult = Email.Create(request.UsernameOrEmail);
            if (emailResult.IsFailure)
            {
                return Result.Failure<LoginResult>(Error.Unauthorized("Invalid username/email or password"));
            }
            user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        }
        else
        {
            user = await GetUserByUsernameStringAsync(request.UsernameOrEmail, cancellationToken);
        }

        if (user is null)
        {
            return Result.Failure<LoginResult>(Error.Unauthorized("Invalid username/email or password"));
        }

        // Get account
        var account = await _accountRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (account is null)
        {
            return Result.Failure<LoginResult>(Error.NotFound("Account", "user"));
        }

        // Check if account is in valid state for login
        var recordLoginResult = account.RecordSuccessfulLogin();
        if (recordLoginResult.IsFailure)
        {
            return Result.Failure<LoginResult>(recordLoginResult.Error);
        }

        // Store Keycloak's refresh token in database
        var expiresAt = _dateTime.UtcNow.AddSeconds(keycloakAuth.ExpiresIn);
        var refreshTokenResult = RefreshTokenEntity.Create(
            Guid.NewGuid(),
            user.Id,
            keycloakAuth.RefreshToken,
            request.DeviceInfo);

        if (refreshTokenResult.IsFailure)
        {
            return Result.Failure<LoginResult>(refreshTokenResult.Error);
        }

        await _refreshTokenRepository.AddAsync(refreshTokenResult.Value, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<LoginResult>(Error.Create("Database.SaveFailed", $"Failed to save login session: {ex.Message}"));
        }

        var result = new LoginResult(
            user.Id,
            keycloakAuth.AccessToken,
            keycloakAuth.RefreshToken,
            expiresAt);

        return Result<LoginResult>.Success(result);
    }

    private async Task<User?> GetUserByUsernameStringAsync(string usernameString, CancellationToken cancellationToken)
    {
        // Parse username string (format: DisplayName#1234)
        var parts = usernameString.Split('#');
        if (parts.Length != 2 || !int.TryParse(parts[1], out var discriminator))
        {
            return null;
        }

        var usernameResult = Username.Create(parts[0], discriminator);
        if (usernameResult.IsFailure)
        {
            return null;
        }

        return await _userRepository.GetByUsernameAsync(usernameResult.Value, cancellationToken);
    }
}
