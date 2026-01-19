using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Identity.Core.Commands;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string DeviceInfo) : ICommand<TokenRefreshResult>;

public sealed record TokenRefreshResult(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);
