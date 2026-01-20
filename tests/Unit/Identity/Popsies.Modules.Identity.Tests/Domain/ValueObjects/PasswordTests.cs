using FluentAssertions;
using Popsies.Modules.Identity.Domain.ValueObjects;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Identity.Tests.Unit.Domain.ValueObjects;

public class PasswordTests
{
    [Theory]
    [InlineData("Password1!", "TestUser#1234")]
    [InlineData("MyP@ssw0rd", "Player#5678")]
    [InlineData("Secure#Pass123", "User#0001")]
    [InlineData("C0mpl3x!Pass", "TestName#9999")]
    public void Create_WithValidPassword_ShouldSucceed(string password, string username)
    {
        // Act
        var result = Password.Create(password, username);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyPassword_ShouldReturnFailure(string? password)
    {
        // Act
        var result = Password.Create(password!, "TestUser#1234");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Password cannot be empty");
    }

    [Theory]
    [InlineData("Short1!")]
    [InlineData("Pass1!")]
    public void Create_WithPasswordTooShort_ShouldReturnFailure(string password)
    {
        // Act
        var result = Password.Create(password, "TestUser#1234");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("must be 8-64 characters");
    }

    [Fact]
    public void Create_WithPasswordTooLong_ShouldReturnFailure()
    {
        // Arrange
        var password = new string('A', 65) + "1!";

        // Act
        var result = Password.Create(password, "TestUser#1234");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("must be 8-64 characters");
    }

    [Theory]
    [InlineData("password1!")]
    [InlineData("mypassword123!")]
    public void Create_WithoutUppercaseLetter_ShouldReturnFailure(string password)
    {
        // Act
        var result = Password.Create(password, "TestUser#1234");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("at least one uppercase letter");
    }

    [Theory]
    [InlineData("PASSWORD1!")]
    [InlineData("MYPASSWORD123!")]
    public void Create_WithoutLowercaseLetter_ShouldReturnFailure(string password)
    {
        // Act
        var result = Password.Create(password, "TestUser#1234");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("at least one lowercase letter");
    }

    [Theory]
    [InlineData("Password!")]
    [InlineData("MyPassword!@#")]
    public void Create_WithoutDigit_ShouldReturnFailure(string password)
    {
        // Act
        var result = Password.Create(password, "TestUser#1234");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("at least one digit");
    }

    [Theory]
    [InlineData("Password1")]
    [InlineData("MyPassword123")]
    public void Create_WithoutSpecialCharacter_ShouldReturnFailure(string password)
    {
        // Act
        var result = Password.Create(password, "TestUser#1234");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("at least one special character");
    }

    [Theory]
    [InlineData("TestUser1234!", "TestUser#1234")]
    [InlineData("Password123!TestUser", "TestUser#5678")]
    [InlineData("Testuser123!", "TestUser#9999")]
    public void Create_WithPasswordContainingUsername_ShouldReturnFailure(string password, string username)
    {
        // Act
        var result = Password.Create(password, username);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("cannot contain the username");
    }

    [Fact]
    public void Password_ShouldNotExposeValue()
    {
        // Arrange
        var password = Password.Create("SecurePass1!", "TestUser#1234").Value;

        // Act
        var stringRepresentation = password.ToString();

        // Assert
        stringRepresentation.Should().Be("********");
    }

    [Fact]
    public void Passwords_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var password1 = Password.Create("SecurePass1!", "TestUser#1234").Value;
        var password2 = Password.Create("SecurePass1!", "TestUser#1234").Value;

        // Act & Assert
        password1.Should().Be(password2);
    }

    [Fact]
    public void Passwords_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var password1 = Password.Create("SecurePass1!", "TestUser#1234").Value;
        var password2 = Password.Create("DifferentPass1!", "TestUser#1234").Value;

        // Act & Assert
        password1.Should().NotBe(password2);
    }
}
