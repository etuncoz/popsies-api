using FluentAssertions;
using Popsies.Modules.Identity.Domain.ValueObjects;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Identity.Tests.Unit.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user+tag@domain.co.uk")]
    [InlineData("valid_email123@test-domain.org")]
    public void Create_WithValidEmail_Sho​uldSucceed(string emailAddress)
    {
        // Act
        var result = Email.Create(emailAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(emailAddress.ToLower());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ShouldReturnFailure(string? emailAddress)
    {
        // Act
        var result = Email.Create(emailAddress!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Email cannot be empty");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user @example.com")]
    [InlineData("user@domain .com")]
    [InlineData("user@@example.com")]
    public void Create_WithInvalidEmailFormat_ShouldReturnFailure(string emailAddress)
    {
        // Act
        var result = Email.Create(emailAddress);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Invalid email format");
    }

    [Fact]
    public void Emails_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var email1 = Email.Create("test@example.com").Value;
        var email2 = Email.Create("TEST@EXAMPLE.COM").Value;

        // Act & Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
    }

    [Fact]
    public void Emails_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com").Value;
        var email2 = Email.Create("test2@example.com").Value;

        // Act & Assert
        email1.Should().NotBe(email2);
        (email1 != email2).Should().BeTrue();
    }

    [Fact]
    public void Email_ShouldStoreValueInLowerCase()
    {
        // Act
        var result = Email.Create("TEST@EXAMPLE.COM");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("test@example.com");
    }
}
