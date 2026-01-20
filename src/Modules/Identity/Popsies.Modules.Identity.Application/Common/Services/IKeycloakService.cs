namespace Popsies.Modules.Identity.Application.Common.Services;

/// <summary>
/// Keycloak integration service for user management and authentication
/// </summary>
public interface IKeycloakService
{
    /// <summary>
    /// Creates a new user in Keycloak with custom attributes
    /// </summary>
    Task<KeycloakUserCreationResult> CreateUserAsync(
        string displayName,
        int discriminator,
        string email,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user via Keycloak password grant
    /// </summary>
    Task<KeycloakAuthResult> AuthenticateAsync(
        string usernameOrEmail,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an access token using a refresh token
    /// </summary>
    Task<KeycloakTokenResult> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates custom user attributes in Keycloak
    /// </summary>
    Task UpdateUserAttributesAsync(
        string keycloakUserId,
        Dictionary<string, string> attributes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes/disables a user in Keycloak
    /// </summary>
    Task DeleteUserAsync(
        string keycloakUserId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of Keycloak user creation
/// </summary>
public record KeycloakUserCreationResult(
    string? KeycloakUserId,
    bool Success,
    string? ErrorMessage = null);

/// <summary>
/// Result of Keycloak authentication
/// </summary>
public record KeycloakAuthResult(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType = "Bearer");

/// <summary>
/// Result of token refresh
/// </summary>
public record KeycloakTokenResult(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn);
