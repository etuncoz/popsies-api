using Popsies.Modules.Identity.Core.Commands;
using Popsies.Modules.Identity.Core.Domain.Entities;
using Popsies.Modules.Identity.Core.Repositories;
using Popsies.Modules.Identity.Core.Services;
using Popsies.Shared.Abstractions.Commands;
using Popsies.Shared.Abstractions.Exceptions;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;
using Popsies.Shared.Abstractions.Time;

namespace Popsies.Modules.Identity.Core.Handlers;

internal sealed class CreateGuestCommandHandler(
    IGuestRepository guestRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IGuestTokenGenerator guestTokenGenerator,
    IUnitOfWork unitOfWork,
    IDateTime dateTime) : ICommandHandler<CreateGuestCommand, GuestCreationResult>
{
    private readonly IGuestRepository _guestRepository = guestRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IGuestTokenGenerator _guestTokenGenerator = guestTokenGenerator;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IDateTime _dateTime = dateTime;

    public async Task<Result<GuestCreationResult>> Handle(CreateGuestCommand command, CancellationToken cancellationToken)
    {
        // Create guest (validation happens in domain entity)
        var guestId = Guid.NewGuid();
        var guestResult = Guest.Create(guestId, command.DisplayName);
        if (guestResult.IsFailure)
        {
            return Result.Failure<GuestCreationResult>(guestResult.Error);
        }

        var guest = guestResult.Value;

        // Generate tokens (local JWT for guests, not Keycloak)
        string accessToken;
        string refreshTokenString;
        try
        {
            accessToken = _guestTokenGenerator.GenerateAccessToken(
                guest.Id,
                $"guest:{command.DisplayName}",
                new[] { "Guest" });

            refreshTokenString = _guestTokenGenerator.GenerateRefreshToken();
        }
        catch (Exception ex)
        {
            return Result.Failure<GuestCreationResult>(Error.Create("TokenGeneration.Failed", $"Failed to generate guest tokens: {ex.Message}"));
        }

        var expirationMinutes = _guestTokenGenerator.GetAccessTokenExpirationMinutes();
        var expiresAt = _dateTime.UtcNow.AddMinutes(expirationMinutes);

        // Create refresh token entity
        var refreshTokenResult = RefreshToken.CreateForGuest(
            Guid.NewGuid(),
            guest.Id,
            refreshTokenString,
            command.DeviceInfo);

        if (refreshTokenResult.IsFailure)
        {
            return Result.Failure<GuestCreationResult>(refreshTokenResult.Error);
        }

        // Persist
        await _guestRepository.AddAsync(guest, cancellationToken);
        await _refreshTokenRepository.AddAsync(refreshTokenResult.Value, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<GuestCreationResult>(Error.Create("Database.SaveFailed", $"Failed to save guest: {ex.Message}"));
        }

        var result = new GuestCreationResult(
            guest.Id,
            accessToken,
            refreshTokenString,
            expiresAt);

        return Result<GuestCreationResult>.Success(result);
    }
}
