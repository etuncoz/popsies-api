using Popsies.Shared.Abstractions.Domain;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Identity.Core.Domain.Entities;

/// <summary>
/// RefreshToken entity - manages JWT refresh tokens
/// Invariants:
/// - RefreshToken has an expiration time (default: 7 days)
/// - Can be revoked
/// - Is rotated on each use (sliding expiration)
/// - User can have multiple active refresh tokens (multiple devices)
/// </summary>
public sealed class RefreshToken : Entity
{
    private const int DefaultExpirationDays = 7;

    public Guid? UserId { get; private set; }
    public Guid? GuestId { get; private set; }
    public string Token { get; private set; }
    public string DeviceInfo { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public bool IsExpired { get; private set; }

    private RefreshToken() { }

    private RefreshToken(Guid id, Guid? userId, Guid? guestId, string token, string deviceInfo) : base(id)
    {
        UserId = userId;
        GuestId = guestId;
        Token = token;
        DeviceInfo = deviceInfo;
        ExpiresAt = DateTime.UtcNow.AddDays(DefaultExpirationDays);
        CreatedAt = DateTime.UtcNow;
        IsRevoked = false;
        IsExpired = false;
    }

    public static Result<RefreshToken> Create(Guid tokenId, Guid userId, string token, string deviceInfo)
    {
        var validationResult = ValidateToken(token);
        if (validationResult.IsFailure)
        {
            return Result.Failure<RefreshToken>(validationResult.Error);
        }

        return Result.Success(new RefreshToken(tokenId, userId, null, token, deviceInfo));
    }

    public static Result<RefreshToken> CreateForGuest(Guid tokenId, Guid guestId, string token, string deviceInfo)
    {
        var validationResult = ValidateToken(token);
        if (validationResult.IsFailure)
        {
            return Result.Failure<RefreshToken>(validationResult.Error);
        }

        return Result.Success(new RefreshToken(tokenId, null, guestId, token, deviceInfo));
    }

    public void Revoke()
    {
        if (!IsRevoked)
        {
            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
        }
    }

    public void CheckExpiration(DateTime currentTime)
    {
        if (!IsExpired && currentTime >= ExpiresAt)
        {
            IsExpired = true;
        }
    }

    public Result Rotate(string newToken)
    {
        var validationResult = ValidateToken(newToken);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        if (IsRevoked)
        {
            return Result.Failure(Error.Create("RefreshToken.Revoked", "Cannot rotate revoked refresh token"));
        }

        if (IsExpired)
        {
            return Result.Failure(Error.Create("RefreshToken.Expired", "Cannot rotate expired refresh token"));
        }

        Token = newToken;
        ExpiresAt = DateTime.UtcNow.AddDays(DefaultExpirationDays);
        LastUsedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public void UpdateLastUsed()
    {
        LastUsedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !IsRevoked && !IsExpired;
    }

    private static Result ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Result.Failure(Error.Validation("Token", "Token cannot be empty"));
        }

        return Result.Success();
    }
}
