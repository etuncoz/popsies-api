using System.Text.RegularExpressions;
using Popsies.Shared.Abstractions.Domain;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Identity.Core.Domain.ValueObjects;

/// <summary>
/// Username value object
/// Format: DisplayName#0000
/// Invariants:
/// - Display name must be 3-20 characters long
/// - Display name can contain alphanumeric characters, underscores, and hyphens
/// - Display name cannot start or end with special characters
/// - Discriminator is a 4-digit number (0001-9999)
/// </summary>
public sealed class Username : ValueObject
{
    private const int MinDisplayNameLength = 3;
    private const int MaxDisplayNameLength = 20;
    private const int MinDiscriminator = 1;
    private const int MaxDiscriminator = 9999;

    private static readonly Regex DisplayNameRegex = new(
        @"^[a-zA-Z0-9][a-zA-Z0-9_-]*[a-zA-Z0-9]$|^[a-zA-Z0-9]{3}$",
        RegexOptions.Compiled);

    public string DisplayName { get; }
    public int Discriminator { get; }
    public string FullUsername => $"{DisplayName}#{Discriminator:D4}";

    private Username(string displayName, int discriminator)
    {
        DisplayName = displayName;
        Discriminator = discriminator;
    }

    public static Result<Username> Create(string displayName, int discriminator)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Result.Failure<Username>(Error.Validation("DisplayName", "Display name cannot be empty"));
        }

        if (displayName.Length < MinDisplayNameLength || displayName.Length > MaxDisplayNameLength)
        {
            return Result.Failure<Username>(Error.Validation("DisplayName",
                $"Display name must be {MinDisplayNameLength}-{MaxDisplayNameLength} characters long"));
        }

        if (!DisplayNameRegex.IsMatch(displayName))
        {
            if (displayName.StartsWith("_") || displayName.StartsWith("-") ||
                displayName.EndsWith("_") || displayName.EndsWith("-"))
            {
                return Result.Failure<Username>(Error.Validation("DisplayName",
                    "Display name cannot start or end with special characters"));
            }

            return Result.Failure<Username>(Error.Validation("DisplayName",
                "Display name can only contain alphanumeric characters, underscores, and hyphens"));
        }

        if (discriminator < MinDiscriminator || discriminator > MaxDiscriminator)
        {
            return Result.Failure<Username>(Error.Validation("Discriminator",
                $"Discriminator must be between {MinDiscriminator} and {MaxDiscriminator}"));
        }

        return Result.Success(new Username(displayName, discriminator));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DisplayName;
        yield return Discriminator;
    }

    public override string ToString() => FullUsername;
}
