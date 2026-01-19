using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Popsies.Modules.Identity.Core.Services;

namespace Popsies.Modules.Identity.Infrastructure.Services;

/// <summary>
/// JWT token generator for guest accounts (local, bypasses Keycloak)
/// </summary>
internal sealed class GuestTokenGenerator : IGuestTokenGenerator
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpirationMinutes;

    public GuestTokenGenerator(IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        _secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        _issuer = jwtSection["Issuer"] ?? "Popsies.Api";
        _audience = jwtSection["Audience"] ?? "Popsies.Client";
        _accessTokenExpirationMinutes = int.Parse(jwtSection["AccessTokenExpirationMinutes"] ?? "1440"); // 24 hours for guests
    }

    public string GenerateAccessToken(Guid guestId, string displayName, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, guestId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, displayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("guest", "true") // Mark as guest token
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public int GetAccessTokenExpirationMinutes()
    {
        return _accessTokenExpirationMinutes;
    }
}
