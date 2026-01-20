using FluentAssertions;
using Popsies.Modules.Identity.Domain.ValueObjects;
using Popsies.Shared.Abstractions.Exceptions;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Identity.Tests.Unit.Domain.ValueObjects;

public class UsernameTests
{
    [Theory]
    [InlineData("User", 1234)]
    [InlineData("Test_User", 1)]
    [InlineData("player-123", 9999)]
    [InlineData("ABC", 1000)]
    [InlineData("User_Name-123", 5678)]
    public void Create_WithValidUsernameAndDiscriminator_ShouldSucceed(string displayName, int discriminator)
    {
        // Act
        var result = Username.Create(displayName, discriminator);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var username = result.Value;
        username.DisplayName.Should().Be(displayName);
        username.Discriminator.Should().Be(discriminator);
        username.FullUsername.Should().Be($"{displayName}#{discriminator:D4}");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyDisplayName_ShouldReturnFailure(string? displayName)
    {
        // Act
        var result = Username.Create(displayName!, 1234);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("Validation");
        result.Error.Message.Should().Contain("Display name cannot be empty");
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("X")]
    public void Create_WithDisplayNameTooShort_ShouldReturnFailure(string displayName)
    {
        // Act
        var result = Username.Create(displayName, 1234);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("Validation");
        result.Error.Message.Should().Contain("must be 3-20 characters");
    }

    [Theory]
    [InlineData("ThisDisplayNameIsTooLongForValidation")]
    [InlineData("ExtremelyLongDisplayNameThatExceedsLimit")]
    public void Create_WithDisplayNameTooLong_ShouldReturnFailure(string displayName)
    {
        // Act
        var result = Username.Create(displayName, 1234);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("Validation");
        result.Error.Message.Should().Contain("must be 3-20 characters");
    }

    [Theory]
    [InlineData("User@Name")]
    [InlineData("Test User")]
    [InlineData("Player!123")]
    [InlineData("User#Name")]
    [InlineData("Test$User")]
    public void Create_WithInvalidCharacters_ShouldReturnFailure(string displayName)
    {
        // Act
        var result = Username.Create(displayName, 1234);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("Validation");
        result.Error.Message.Should().Contain("alphanumeric characters, underscores, and hyphens");
    }

    [Theory]
    [InlineData("_UserName")]
    [InlineData("-TestUser")]
    public void Create_WithDisplayNameStartingWithSpecialChar_ShouldReturnFailure(string displayName)
    {
        // Act
        var result = Username.Create(displayName, 1234);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("Validation");
        result.Error.Message.Should().Contain("cannot start or end with special characters");
    }

    [Theory]
    [InlineData("UserName_")]
    [InlineData("TestUser-")]
    public void Create_WithDisplayNameEndingWithSpecialChar_ShouldReturnFailure(string displayName)
    {
        // Act
        var result = Username.Create(displayName, 1234);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("Validation");
        result.Error.Message.Should().Contain("cannot start or end with special characters");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10000)]
    [InlineData(99999)]
    public void Create_WithDiscriminatorOutOfRange_ShouldReturnFailure(int discriminator)
    {
        // Act
        var result = Username.Create("TestUser", discriminator);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().StartWith("Validation");
        result.Error.Message.Should().Contain("must be between 1 and 9999");
    }

    [Fact]
    public void Usernames_WithSameDisplayNameAndDiscriminator_ShouldBeEqual()
    {
        // Arrange
        var username1 = Username.Create("TestUser", 1234).Value;
        var username2 = Username.Create("TestUser", 1234).Value;

        // Act & Assert
        username1.Should().Be(username2);
        (username1 == username2).Should().BeTrue();
    }

    [Fact]
    public void Usernames_WithDifferentDiscriminators_ShouldNotBeEqual()
    {
        // Arrange
        var username1 = Username.Create("TestUser", 1234).Value;
        var username2 = Username.Create("TestUser", 5678).Value;

        // Act & Assert
        username1.Should().NotBe(username2);
        (username1 != username2).Should().BeTrue();
    }

    [Fact]
    public void Usernames_WithDifferentDisplayNames_ShouldNotBeEqual()
    {
        // Arrange
        var username1 = Username.Create("TestUser1", 1234).Value;
        var username2 = Username.Create("TestUser2", 1234).Value;

        // Act & Assert
        username1.Should().NotBe(username2);
        (username1 != username2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedUsername()
    {
        // Arrange
        var username = Username.Create("TestUser", 123).Value;

        // Act
        var result = username.ToString();

        // Assert
        result.Should().Be("TestUser#0123");
    }

    [Fact]
    public void FullUsername_ShouldFormatDiscriminatorWithLeadingZeros()
    {
        // Arrange
        var username = Username.Create("TestUser", 1).Value;

        // Act & Assert
        username.FullUsername.Should().Be("TestUser#0001");
    }
}
