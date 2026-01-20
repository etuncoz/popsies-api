namespace Popsies.Modules.Identity.Application.Common.Services;

/// <summary>
/// Token generator for guest accounts (bypasses Keycloak)
/// </summary>
public interface IGuestTokenGenerator
{
    string GenerateAccessToken(Guid guestId, string displayName, IEnumerable<string> roles);
    string GenerateRefreshToken();
    int GetAccessTokenExpirationMinutes();
}
