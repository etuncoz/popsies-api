using System.Text.RegularExpressions;
using Popsies.Shared.Abstractions.Domain;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Identity.Core.Domain.ValueObjects;

/// <summary>
/// Email value object
/// Invariant: Email must be a valid email format
/// </summary>
public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<Email>(Error.Validation("Email", "Email cannot be empty"));
        }

        var normalizedEmail = email.Trim().ToLower();

        if (!EmailRegex.IsMatch(normalizedEmail))
        {
            return Result.Failure<Email>(Error.Validation("Email", "Invalid email format"));
        }

        return Result.Success(new Email(normalizedEmail));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
