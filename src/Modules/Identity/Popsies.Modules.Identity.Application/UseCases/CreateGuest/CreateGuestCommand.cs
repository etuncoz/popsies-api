using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Identity.Application.UseCases.CreateGuest;

public sealed record CreateGuestCommand(
    string DisplayName,
    string DeviceInfo) : ICommand<GuestCreationResult>;

public sealed record GuestCreationResult(
    Guid GuestId,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);
