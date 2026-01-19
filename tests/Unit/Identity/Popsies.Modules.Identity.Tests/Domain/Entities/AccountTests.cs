using FluentAssertions;
using Popsies.Modules.Identity.Core.Domain.Entities;
using Popsies.Modules.Identity.Core.Domain.Enums;
using Popsies.Modules.Identity.Core.Domain.ValueObjects;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Identity.Tests.Unit.Domain.Entities;

public class AccountTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateAccountInPendingState()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var username = Username.Create("TestUser", 1234).Value;
        var email = Email.Create("test@example.com").Value;

        // Act
        var account = Account.Create(accountId, userId, username, email);

        // Assert
        account.Id.Should().Be(accountId);
        account.UserId.Should().Be(userId);
        account.Username.Should().Be(username);
        account.Email.Should().Be(email);
        account.State.Should().Be(AccountState.Pending);
        account.IsEmailVerified.Should().BeFalse();
        account.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public void VerifyEmail_WhenPending_ShouldActivateAccount()
    {
        // Arrange
        var account = CreateTestAccount();

        // Act
        account.VerifyEmail();

        // Assert
        account.IsEmailVerified.Should().BeTrue();
        account.State.Should().Be(AccountState.Active);
    }

    [Fact]
    public void VerifyEmail_WhenAlreadyVerified_ShouldNotThrow()
    {
        // Arrange
        var account = CreateTestAccount();
        account.VerifyEmail();

        // Act
        var act = () => account.VerifyEmail();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordSuccessfulLogin_ShouldUpdateLastLogin()
    {
        // Arrange
        var account = CreateTestAccount();
        account.VerifyEmail();

        // Act
        var result = account.RecordSuccessfulLogin();

        // Assert
        result.IsSuccess.Should().BeTrue();
        account.LastLoginAt.Should().NotBeNull();
        account.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Suspend_WhenActive_ShouldSuspendAccount()
    {
        // Arrange
        var account = CreateTestAccount();
        account.VerifyEmail();

        // Act
        account.Suspend();

        // Assert
        account.State.Should().Be(AccountState.Suspended);
    }

    [Fact]
    public void Suspend_WhenAlreadySuspended_ShouldNotThrow()
    {
        // Arrange
        var account = CreateTestAccount();
        account.VerifyEmail();
        account.Suspend();

        // Act
        var act = () => account.Suspend();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Activate_WhenSuspended_ShouldActivateAccount()
    {
        // Arrange
        var account = CreateTestAccount();
        account.VerifyEmail();
        account.Suspend();

        // Act
        account.Activate();

        // Assert
        account.State.Should().Be(AccountState.Active);
    }

    [Fact]
    public void Delete_WhenNotDeleted_ShouldMarkAsDeleted()
    {
        // Arrange
        var account = CreateTestAccount();

        // Act
        account.Delete();

        // Assert
        account.State.Should().Be(AccountState.Deleted);
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ShouldNotThrow()
    {
        // Arrange
        var account = CreateTestAccount();
        account.Delete();

        // Act
        var act = () => account.Delete();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordSuccessfulLogin_WhenDeleted_ShouldReturnFailure()
    {
        // Arrange
        var account = CreateTestAccount();
        account.Delete();

        // Act
        var result = account.RecordSuccessfulLogin();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("deleted account");
    }

    [Fact]
    public void RecordSuccessfulLogin_WhenSuspended_ShouldReturnFailure()
    {
        // Arrange
        var account = CreateTestAccount();
        account.VerifyEmail();
        account.Suspend();

        // Act
        var result = account.RecordSuccessfulLogin();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("suspended");
    }

    private static Account CreateTestAccount()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var username = Username.Create("TestUser", 1234).Value;
        var email = Email.Create("test@example.com").Value;

        return Account.Create(accountId, userId, username, email);
    }
}
