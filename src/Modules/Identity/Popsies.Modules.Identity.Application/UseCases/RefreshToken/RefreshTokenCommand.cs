using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Identity.Application.UseCases.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string DeviceInfo) : ICommand<TokenRefreshResult>;

public sealed record TokenRefreshResult(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);
