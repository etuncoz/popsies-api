using FluentAssertions;
using Popsies.Modules.Identity.Domain.Accounts;
using Popsies.Modules.Identity.Domain.Guests;
using Popsies.Modules.Identity.Domain.RefreshTokens;
using Popsies.Modules.Identity.Domain.Users.Events;
using Popsies.Modules.Identity.Domain.Guests.Events;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Identity.Tests.Unit.Domain.Entities;

public class GuestTests
{
    private const int DefaultExpirationHours = 24;

    [Fact]
    public void Create_WithValidDisplayName_ShouldCreateGuestAndRaiseEvent()
    {
        // Arrange
        var guestId = Guid.NewGuid();
        var displayName = "GuestUser123";

        // Act
        var guest = Guest.Create(guestId, displayName).Value;

        // Assert
        guest.Id.Should().Be(guestId);
        guest.DisplayName.Should().Be(displayName);
        guest.IsExpired.Should().BeFalse();
        guest.IsConverted.Should().BeFalse();
        guest.ConvertedToUserId.Should().BeNull();
        guest.ExpiresAt.Should().BeCloseTo(
            DateTime.UtcNow.AddHours(DefaultExpirationHours),
            TimeSpan.FromSeconds(1));

        guest.DomainEvents.Should().ContainSingle();
        guest.DomainEvents.Should().ContainItemsAssignableTo<GuestCreatedEvent>();

        var domainEvent = guest.DomainEvents.First() as GuestCreatedEvent;
        domainEvent!.GuestId.Should().Be(guestId);
        domainEvent.DisplayName.Should().Be(displayName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyDisplayName_ShouldThrowValidationException(string? displayName)
    {
        // Arrange
        var guestId = Guid.NewGuid();

        // Act
        var result = Guest.Create(guestId, displayName!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation.DisplayName");
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("X")]
    public void Create_WithDisplayNameTooShort_ShouldThrowValidationException(string displayName)
    {
        // Arrange
        var guestId = Guid.NewGuid();

        // Act
        var result = Guest.Create(guestId, displayName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation.DisplayName");
    }

    [Fact]
    public void Create_WithDisplayNameTooLong_ShouldThrowValidationException()
    {
        // Arrange
        var guestId = Guid.NewGuid();
        var displayName = new string('A', 21);

        // Act
        var result = Guest.Create(guestId, displayName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation.DisplayName");
    }

    [Fact]
    public void CheckExpiration_WhenExpired_ShouldMarkAsExpiredAndRaiseEvent()
    {
        // Arrange
        var guest = CreateTestGuest();
        var futureTime = DateTime.UtcNow.AddHours(DefaultExpirationHours + 1);

        // Act
        guest.CheckExpiration(futureTime);

        // Assert
        guest.IsExpired.Should().BeTrue();
        guest.DomainEvents.Should().HaveCount(2); // GuestCreated + GuestExpired
        guest.DomainEvents.Last().Should().BeOfType<GuestExpiredEvent>();

        var expiredEvent = guest.DomainEvents.Last() as GuestExpiredEvent;
        expiredEvent!.GuestId.Should().Be(guest.Id);
    }

    [Fact]
    public void CheckExpiration_WhenNotExpired_ShouldNotMarkAsExpired()
    {
        // Arrange
        var guest = CreateTestGuest();
        var nearFutureTime = DateTime.UtcNow.AddHours(12);

        // Act
        guest.CheckExpiration(nearFutureTime);

        // Assert
        guest.IsExpired.Should().BeFalse();
        guest.DomainEvents.Should().ContainSingle(); // Only GuestCreated
    }

    [Fact]
    public void CheckExpiration_WhenAlreadyExpired_ShouldNotRaiseDuplicateEvent()
    {
        // Arrange
        var guest = CreateTestGuest();
        var futureTime = DateTime.UtcNow.AddHours(DefaultExpirationHours + 1);
        guest.CheckExpiration(futureTime);
        guest.ClearDomainEvents();

        // Act
        guest.CheckExpiration(futureTime);

        // Assert
        guest.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ConvertToUser_WhenNotConverted_ShouldConvertAndRaiseEvent()
    {
        // Arrange
        var guest = CreateTestGuest();
        var userId = Guid.NewGuid();

        // Act
        var result = guest.ConvertToUser(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        guest.IsConverted.Should().BeTrue();
        guest.ConvertedToUserId.Should().Be(userId);
        guest.DomainEvents.Should().HaveCount(2); // GuestCreated + GuestConverted
        guest.DomainEvents.Last().Should().BeOfType<GuestConvertedEvent>();

        var convertedEvent = guest.DomainEvents.Last() as GuestConvertedEvent;
        convertedEvent!.GuestId.Should().Be(guest.Id);
        convertedEvent.UserId.Should().Be(userId);
    }

    [Fact]
    public void ConvertToUser_WhenAlreadyConverted_ShouldReturnFailure()
    {
        // Arrange
        var guest = CreateTestGuest();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        guest.ConvertToUser(userId1);

        // Act
        var result = guest.ConvertToUser(userId2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("already been converted");
    }

    [Fact]
    public void ConvertToUser_WhenExpired_ShouldReturnFailure()
    {
        // Arrange
        var guest = CreateTestGuest();
        var futureTime = DateTime.UtcNow.AddHours(DefaultExpirationHours + 1);
        guest.CheckExpiration(futureTime);
        var userId = Guid.NewGuid();

        // Act
        var result = guest.ConvertToUser(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("expired guest");
    }

    [Fact]
    public void StartQuizSession_WhenNoActiveSession_ShouldSetActiveSession()
    {
        // Arrange
        var guest = CreateTestGuest();
        var sessionId = Guid.NewGuid();

        // Act
        var result = guest.StartQuizSession(sessionId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        guest.ActiveQuizSessionId.Should().Be(sessionId);
    }

    [Fact]
    public void StartQuizSession_WhenAlreadyHasActiveSession_ShouldReturnFailure()
    {
        // Arrange
        var guest = CreateTestGuest();
        var sessionId1 = Guid.NewGuid();
        var sessionId2 = Guid.NewGuid();
        guest.StartQuizSession(sessionId1);

        // Act
        var result = guest.StartQuizSession(sessionId2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("already participating");
    }

    [Fact]
    public void EndQuizSession_WhenHasActiveSession_ShouldClearActiveSession()
    {
        // Arrange
        var guest = CreateTestGuest();
        var sessionId = Guid.NewGuid();
        guest.StartQuizSession(sessionId);

        // Act
        guest.EndQuizSession();

        // Assert
        guest.ActiveQuizSessionId.Should().BeNull();
    }

    private static Guest CreateTestGuest()
    {
        var guestId = Guid.NewGuid();
        var displayName = "TestGuest123";
        return Guest.Create(guestId, displayName).Value;
    }
}
