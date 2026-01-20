using FluentAssertions;
using Popsies.Modules.Identity.Domain.Accounts;
using Popsies.Modules.Identity.Domain.Guests;
using Popsies.Modules.Identity.Domain.RefreshTokens;
using Popsies.Shared.Abstractions.Exceptions;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Identity.Tests.Unit.Domain.Entities;

public class RefreshTokenTests
{
    private const int DefaultExpirationDays = 7;

    [Fact]
    public void Create_WithValidData_ShouldCreateRefreshToken()
    {
        // Arrange
        var tokenId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var token = "refresh_token_value";
        var deviceInfo = "Chrome/Windows";

        // Act
        var result = RefreshToken.Create(tokenId, userId, token, deviceInfo);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var refreshToken = result.Value;
        refreshToken.Id.Should().Be(tokenId);
        refreshToken.UserId.Should().Be(userId);
        refreshToken.Token.Should().Be(token);
        refreshToken.DeviceInfo.Should().Be(deviceInfo);
        refreshToken.IsRevoked.Should().BeFalse();
        refreshToken.IsExpired.Should().BeFalse();
        refreshToken.ExpiresAt.Should().BeCloseTo(
            DateTime.UtcNow.AddDays(DefaultExpirationDays),
            TimeSpan.FromSeconds(1));
        refreshToken.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ForGuest_ShouldCreateRefreshTokenWithGuestId()
    {
        // Arrange
        var tokenId = Guid.NewGuid();
        var guestId = Guid.NewGuid();
        var token = "refresh_token_value";
        var deviceInfo = "Safari/MacOS";

        // Act
        var result = RefreshToken.CreateForGuest(tokenId, guestId, token, deviceInfo);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var refreshToken = result.Value;
        refreshToken.Id.Should().Be(tokenId);
        refreshToken.GuestId.Should().Be(guestId);
        refreshToken.UserId.Should().BeNull();
        refreshToken.Token.Should().Be(token);
        refreshToken.DeviceInfo.Should().Be(deviceInfo);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyToken_ShouldReturnFailure(string? token)
    {
        // Arrange
        var tokenId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var result = RefreshToken.Create(tokenId, userId, token!, "device");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("Validation");
        result.Error.Message.Should().Contain("Token cannot be empty");
    }

    [Fact]
    public void Revoke_WhenNotRevoked_ShouldMarkAsRevoked()
    {
        // Arrange
        var refreshToken = CreateTestRefreshToken();

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.IsRevoked.Should().BeTrue();
        refreshToken.RevokedAt.Should().NotBeNull();
        refreshToken.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ShouldNotThrow()
    {
        // Arrange
        var refreshToken = CreateTestRefreshToken();
        refreshToken.Revoke();
        var firstRevokedAt = refreshToken.RevokedAt;

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.RevokedAt.Should().Be(firstRevokedAt);
    }

    [Fact]
    public void CheckExpiration_WhenExpired_ShouldMarkAsExpired()
    {
        // Arrange
        var refreshToken = CreateTestRefreshToken();
        var futureTime = DateTime.UtcNow.AddDays(DefaultExpirationDays + 1);

        // Act
        refreshToken.CheckExpiration(futureTime);

        // Assert
        refreshToken.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void CheckExpiration_WhenNotExpired_ShouldNotMarkAsExpired()
    {
        // Arrange
        var refreshToken = CreateTestRefreshToken();
        var nearFutureTime = DateTime.UtcNow.AddDays(3);

        // Act
        refreshToken.CheckExpiration(nearFutureTime);

        // Assert
        refreshToken.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void Rotate_WhenValid_ShouldUpdateTokenAndExtendExpiration()
    {
        // Arrange
        var refreshToken = CreateTestRefreshToken();
        var newToken = "new_refresh_token_value";
        var originalExpiresAt = refreshToken.ExpiresAt;

        // Act
        var result = refreshToken.Rotate(newToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        refreshToken.Token.Should().Be(newToken);
        refreshToken.ExpiresAt.Should().BeAfter(originalExpiresAt);
        refreshToken.ExpiresAt.Should().BeCloseTo(
            DateTime.UtcNow.AddDays(DefaultExpirationDays),
            TimeSpan.FromSeconds(1));
        refreshToken.LastUsedAt.Should().NotBeNull();
        refreshToken.LastUsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Rotate_WithEmptyToken_ShouldReturnFailure(string? newToken)
    {
        // Arrange
        var refreshToken = CreateTestRefreshToken();

        // Act
        var result = refreshToken.Rotate(newToken!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("Validation");
        result.Error.Message.Should().Contain("Token cannot be empty");
    }

    [Fact]
    public void Rotate_WhenRevoked_ShouldReturnFailure()
    {
        // Arrange
        var refreshToken = CreateTestRefreshToken();
        refreshToken.Revoke();

        // Act
        var result = refreshToken.Rotate("new_token");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RefreshToken.Revoked");
        result.Error.Message.Should().Contain("revoked");
    }

    [Fact]
    public void Rotate_WhenExpired_ShouldReturnFailure()
    {
        // Arrange
        var refreshToken = CreateTestRefreshToken();
        var futureTime = DateTime.UtcNow.AddDays(DefaultExpirationDays + 1);
        refreshToken.CheckExpiration(futureTime);

        // Act
        var result = refreshToken.Rotate("new_token");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RefreshToken.Expired");
        result.Error.Message.Should().Contain("expired");
    }

    [Fact]
    public void UpdateLastUsed_ShouldUpdateTimestamp()
    {
        // Arrange
        var refreshToken = CreateTestRefreshToken();
        var initialLastUsed = refreshToken.LastUsedAt;

        // Act
        Thread.Sleep(10); // Small delay to ensure time difference
        refreshToken.UpdateLastUsed();

        // Assert
        refreshToken.LastUsedAt.Should().NotBe(initialLastUsed);
        refreshToken.LastUsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void IsValid_WhenNotRevokedAndNotExpired_ShouldReturnTrue()
    {
        // Arrange
        var refreshToken = CreateTestRefreshToken();

        // Act
        var isValid = refreshToken.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WhenRevoked_ShouldReturnFalse()
    {
        // Arrange
        var refreshToken = CreateTestRefreshToken();
        refreshToken.Revoke();

        // Act
        var isValid = refreshToken.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        var refreshToken = CreateTestRefreshToken();
        var futureTime = DateTime.UtcNow.AddDays(DefaultExpirationDays + 1);
        refreshToken.CheckExpiration(futureTime);

        // Act
        var isValid = refreshToken.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    private static RefreshToken CreateTestRefreshToken()
    {
        var tokenId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var token = "test_refresh_token_value";
        var deviceInfo = "Chrome/Windows";

        return RefreshToken.Create(tokenId, userId, token, deviceInfo).Value;
    }
}
