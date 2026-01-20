namespace Popsies.Shared.Abstractions.Users;

/// <summary>
/// Provides access to the currently authenticated user
/// </summary>
public interface ICurrentUserProvider
{
    /// <summary>
    /// Gets the current authenticated user from the HTTP context
    /// </summary>
    /// <returns>The current user with ID, permissions, and roles</returns>
    CurrentUser GetCurrentUser();
}
