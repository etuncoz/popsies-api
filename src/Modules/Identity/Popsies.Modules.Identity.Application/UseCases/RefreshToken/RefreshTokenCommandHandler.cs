using MediatR;
using Popsies.Modules.Identity.Application.Common.Repositories;
using Popsies.Modules.Identity.Application.Common.Services;
using Popsies.Shared.Abstractions.Exceptions;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;
using Popsies.Shared.Abstractions.Time;
using RefreshTokenEntity = Popsies.Modules.Identity.Domain.RefreshTokens.RefreshToken;

namespace Popsies.Modules.Identity.Application.UseCases.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IKeycloakService keycloakService,
    IUnitOfWork unitOfWork,
    IDateTime dateTime) : IRequestHandler<RefreshTokenCommand, Result<TokenRefreshResult>>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IDateTime _dateTime = dateTime;

    public async Task<Result<TokenRefreshResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Get refresh token from database
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (refreshToken is null)
        {
            return Result.Failure<TokenRefreshResult>(Error.Unauthorized("Invalid refresh token"));
        }

        if (refreshToken.IsRevoked)
        {
            return Result.Failure<TokenRefreshResult>(Error.Unauthorized("Refresh token has been revoked"));
        }

        // Check expiration
        refreshToken.CheckExpiration(_dateTime.UtcNow);
        if (refreshToken.IsExpired)
        {
            return Result.Failure<TokenRefreshResult>(Error.Unauthorized("Refresh token has expired"));
        }

        // Get user (supports both user and guest tokens)
        var userId = refreshToken.UserId;
        if (userId is null)
        {
            return Result.Failure<TokenRefreshResult>(Error.Unauthorized("Invalid refresh token: no user associated"));
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user is null)
        {
            return Result.Failure<TokenRefreshResult>(Error.NotFound("User", "refresh token"));
        }

        // Refresh tokens via Keycloak
        KeycloakTokenResult keycloakTokens;
        try
        {
            keycloakTokens = await _keycloakService.RefreshTokenAsync(
                request.RefreshToken,
                cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            return Result.Failure<TokenRefreshResult>(Error.Unauthorized("Invalid or expired refresh token"));
        }
        catch (Exception ex)
        {
            return Result.Failure<TokenRefreshResult>(Error.Create("TokenRefresh.Failed", $"Token refresh failed: {ex.Message}"));
        }

        // Revoke old refresh token
        refreshToken.Revoke();

        // Create new refresh token entity
        var expiresAt = _dateTime.UtcNow.AddSeconds(keycloakTokens.ExpiresIn);
        var newRefreshTokenResult = RefreshTokenEntity.Create(
            Guid.NewGuid(),
            user.Id,
            keycloakTokens.RefreshToken,
            request.DeviceInfo);

        if (newRefreshTokenResult.IsFailure)
        {
            return Result.Failure<TokenRefreshResult>(newRefreshTokenResult.Error);
        }

        await _refreshTokenRepository.AddAsync(newRefreshTokenResult.Value, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<TokenRefreshResult>(Error.Create("Database.SaveFailed", $"Failed to save refresh token: {ex.Message}"));
        }

        var result = new TokenRefreshResult(
            keycloakTokens.AccessToken,
            keycloakTokens.RefreshToken,
            expiresAt);

        return Result<TokenRefreshResult>.Success(result);
    }
}
