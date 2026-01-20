using MediatR;
using Popsies.Modules.Identity.Application.Common.Repositories;
using Popsies.Modules.Identity.Application.Common.Services;
using Popsies.Modules.Identity.Domain.Guests;
using Popsies.Shared.Abstractions.Exceptions;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;
using Popsies.Shared.Abstractions.Time;
using RefreshTokenEntity = Popsies.Modules.Identity.Domain.RefreshTokens.RefreshToken;

namespace Popsies.Modules.Identity.Application.UseCases.CreateGuest;

public sealed class CreateGuestCommandHandler(
    IGuestRepository guestRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IGuestTokenGenerator guestTokenGenerator,
    IUnitOfWork unitOfWork,
    IDateTime dateTime) : IRequestHandler<CreateGuestCommand, Result<GuestCreationResult>>
{
    private readonly IGuestRepository _guestRepository = guestRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IGuestTokenGenerator _guestTokenGenerator = guestTokenGenerator;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IDateTime _dateTime = dateTime;

    public async Task<Result<GuestCreationResult>> Handle(CreateGuestCommand request, CancellationToken cancellationToken)
    {
        // Create guest (validation happens in domain entity)
        var guestId = Guid.NewGuid();
        var guestResult = Guest.Create(guestId, request.DisplayName);
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
                $"guest:{request.DisplayName}",
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
        var refreshTokenResult = RefreshTokenEntity.CreateForGuest(
            Guid.NewGuid(),
            guest.Id,
            refreshTokenString,
            request.DeviceInfo);

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
