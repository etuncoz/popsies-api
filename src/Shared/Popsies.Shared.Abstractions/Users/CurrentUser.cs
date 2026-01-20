namespace Popsies.Shared.Abstractions.Users;

/// <summary>
/// Represents the currently authenticated user
/// </summary>
public record CurrentUser(
    Guid Id,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> Roles);
