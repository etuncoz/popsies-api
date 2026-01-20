namespace Popsies.Modules.Identity.Application.Common.Services;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Guid userId, string username, string email, IEnumerable<string> roles);
    string GenerateRefreshToken();
    int GetAccessTokenExpirationMinutes();
}
