using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Identity.Application.UseCases.Login;

public sealed record LoginCommand(
    string UsernameOrEmail,
    string Password,
    string DeviceInfo) : ICommand<LoginResult>;

public sealed record LoginResult(
    Guid UserId,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);
