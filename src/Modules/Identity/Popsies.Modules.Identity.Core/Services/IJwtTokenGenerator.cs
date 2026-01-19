namespace Popsies.Modules.Identity.Core.Services;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Guid userId, string username, string email, IEnumerable<string> roles);
    string GenerateRefreshToken();
    int GetAccessTokenExpirationMinutes();
}
