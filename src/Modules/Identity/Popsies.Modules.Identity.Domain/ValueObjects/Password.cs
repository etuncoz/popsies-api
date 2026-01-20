using System.Text.RegularExpressions;
using Popsies.Shared.Abstractions.Domain;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Identity.Domain.ValueObjects;

/// <summary>
/// Password value object
/// Invariants:
/// - Must be 8-64 characters long
/// - Must contain at least one uppercase letter
/// - Must contain at least one lowercase letter
/// - Must contain at least one digit
/// - Must contain at least one special character
/// - Cannot contain the username
/// </summary>
public sealed class Password : ValueObject
{
    private const int MinLength = 8;
    private const int MaxLength = 64;
    private const string SpecialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?";

    private static readonly Regex UppercaseRegex = new(@"[A-Z]", RegexOptions.Compiled);
    private static readonly Regex LowercaseRegex = new(@"[a-z]", RegexOptions.Compiled);
    private static readonly Regex DigitRegex = new(@"\d", RegexOptions.Compiled);

    public string Value { get; }

    private Password(string value)
    {
        Value = value;
    }

    public static Result<Password> Create(string password, string username)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return Result.Failure<Password>(Error.Validation("Password", "Password cannot be empty"));
        }

        if (password.Length < MinLength || password.Length > MaxLength)
        {
            return Result.Failure<Password>(Error.Validation("Password",
                $"Password must be {MinLength}-{MaxLength} characters long"));
        }

        if (!UppercaseRegex.IsMatch(password))
        {
            return Result.Failure<Password>(Error.Validation("Password",
                "Password must contain at least one uppercase letter"));
        }

        if (!LowercaseRegex.IsMatch(password))
        {
            return Result.Failure<Password>(Error.Validation("Password",
                "Password must contain at least one lowercase letter"));
        }

        if (!DigitRegex.IsMatch(password))
        {
            return Result.Failure<Password>(Error.Validation("Password",
                "Password must contain at least one digit"));
        }

        if (!password.Any(c => SpecialCharacters.Contains(c)))
        {
            return Result.Failure<Password>(Error.Validation("Password",
                $"Password must contain at least one special character ({SpecialCharacters})"));
        }

        // Extract display name from username (format: DisplayName#1234)
        var displayName = username.Split('#')[0];
        if (password.Contains(displayName, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<Password>(Error.Validation("Password",
                "Password cannot contain the username"));
        }

        return Result.Success(new Password(password));
    }

    public static Result<Password> FromHash(string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            return Result.Failure<Password>(Error.Validation("Password", "Hashed password cannot be empty"));
        }

        return Result.Success(new Password(hashedPassword));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => "********";
}
