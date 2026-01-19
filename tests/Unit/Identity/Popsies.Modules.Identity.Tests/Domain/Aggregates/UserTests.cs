using FluentAssertions;
using Popsies.Modules.Identity.Core.Domain.Aggregates;
using Popsies.Modules.Identity.Core.Domain.Events;
using Popsies.Modules.Identity.Core.Domain.ValueObjects;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Identity.Tests.Unit.Unit.Domain.Aggregates;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUserAndRaiseEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = Username.Create("TestUser", 1234).Value;
        var email = Email.Create("test@example.com").Value;

        // Act
        var user = User.Create(userId, username, email);

        // Assert
        user.Id.Should().Be(userId);
        user.Username.Should().Be(username);
        user.Email.Should().Be(email);
        user.IsDeleted.Should().BeFalse();
        user.DisplayName.Should().Be(username.DisplayName);
        user.AvatarUrl.Should().BeNull();

        user.DomainEvents.Should().ContainSingle();
        user.DomainEvents.Should().ContainItemsAssignableTo<UserRegisteredEvent>();

        var domainEvent = user.DomainEvents.First() as UserRegisteredEvent;
        domainEvent!.UserId.Should().Be(userId);
        domainEvent.Username.Should().Be(username.FullUsername);
        domainEvent.Email.Should().Be(email.Value);
    }

    [Fact]
    public void UpdateProfile_WithValidData_ShouldUpdateAndRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var newDisplayName = "UpdatedName";
        var newAvatarUrl = "https://example.com/avatar.jpg";

        // Act
        var result = user.UpdateProfile(newDisplayName, newAvatarUrl);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.DisplayName.Should().Be(newDisplayName);
        user.AvatarUrl.Should().Be(newAvatarUrl);
        user.UpdatedAt.Should().NotBeNull();

        user.DomainEvents.Should().HaveCount(2); // UserRegistered + UserProfileUpdated
        user.DomainEvents.Last().Should().BeOfType<UserProfileUpdatedEvent>();

        var profileEvent = user.DomainEvents.Last() as UserProfileUpdatedEvent;
        profileEvent!.UserId.Should().Be(user.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateProfile_WithEmptyDisplayName_ShouldReturnFailure(string? displayName)
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var result = user.UpdateProfile(displayName!, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Display name cannot be empty");
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("X")]
    public void UpdateProfile_WithDisplayNameTooShort_ShouldReturnFailure(string displayName)
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var result = user.UpdateProfile(displayName, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("must be 3-20 characters");
    }

    [Fact]
    public void UpdateProfile_WithDisplayNameTooLong_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var displayName = new string('A', 21);

        // Act
        var result = user.UpdateProfile(displayName, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("must be 3-20 characters");
    }

    [Theory]
    [InlineData("https://example.com/avatar.jpg")]
    [InlineData("https://cdn.example.com/avatars/user123.png")]
    [InlineData(null)]
    public void UpdateProfile_WithValidAvatarUrl_ShouldSucceed(string? avatarUrl)
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var result = user.UpdateProfile("NewName", avatarUrl);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.AvatarUrl.Should().Be(avatarUrl);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/avatar.jpg")]
    [InlineData("javascript:alert('xss')")]
    public void UpdateProfile_WithInvalidAvatarUrl_ShouldReturnFailure(string avatarUrl)
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var result = user.UpdateProfile("NewName", avatarUrl);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Invalid avatar URL");
    }

    [Fact]
    public void Delete_WhenNotDeleted_ShouldMarkAsDeletedAndRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var result = user.Delete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsDeleted.Should().BeTrue();
        user.DeletedAt.Should().NotBeNull();
        user.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        user.DomainEvents.Should().HaveCount(2); // UserRegistered + UserDeleted
        user.DomainEvents.Last().Should().BeOfType<UserDeletedEvent>();

        var deletedEvent = user.DomainEvents.Last() as UserDeletedEvent;
        deletedEvent!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        user.Delete();

        // Act
        var result = user.Delete();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("already deleted");
    }

    [Fact]
    public void UpdateProfile_WhenDeleted_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        user.Delete();

        // Act
        var result = user.UpdateProfile("NewName", null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("deleted user");
    }

    [Fact]
    public void StartQuizSession_WhenNoActiveSession_ShouldSetActiveSession()
    {
        // Arrange
        var user = CreateTestUser();
        var sessionId = Guid.NewGuid();

        // Act
        var result = user.StartQuizSession(sessionId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.ActiveQuizSessionId.Should().Be(sessionId);
    }

    [Fact]
    public void StartQuizSession_WhenAlreadyHasActiveSession_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var sessionId1 = Guid.NewGuid();
        var sessionId2 = Guid.NewGuid();
        user.StartQuizSession(sessionId1);

        // Act
        var result = user.StartQuizSession(sessionId2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("already participating");
    }

    [Fact]
    public void EndQuizSession_WhenHasActiveSession_ShouldClearActiveSession()
    {
        // Arrange
        var user = CreateTestUser();
        var sessionId = Guid.NewGuid();
        user.StartQuizSession(sessionId);

        // Act
        user.EndQuizSession();

        // Assert
        user.ActiveQuizSessionId.Should().BeNull();
    }

    [Fact]
    public void EndQuizSession_WhenNoActiveSession_ShouldNotThrow()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var act = () => user.EndQuizSession();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void UpdateStatistics_ShouldUpdateValues()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        user.UpdateStatistics(totalQuizzes: 10, wins: 5, averageScore: 85.5);

        // Assert
        user.TotalQuizzesPlayed.Should().Be(10);
        user.TotalWins.Should().Be(5);
        user.AverageScore.Should().Be(85.5);
    }

    private static User CreateTestUser()
    {
        var userId = Guid.NewGuid();
        var username = Username.Create("TestUser", 1234).Value;
        var email = Email.Create("test@example.com").Value;
        return User.Create(userId, username, email);
    }
}
