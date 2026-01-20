using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Popsies.Shared.Abstractions.Users;

namespace Popsies.Shared.Infrastructure.Users;

/// <summary>
/// Provides access to the currently authenticated user from HTTP context
/// </summary>
public sealed class CurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the current authenticated user from JWT claims
    /// </summary>
    /// <returns>CurrentUser with ID, permissions, and roles</returns>
    /// <exception cref="InvalidOperationException">Thrown when HttpContext is null or user is not authenticated</exception>
    public CurrentUser GetCurrentUser()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HTTP context is not available");

        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
        {
            throw new InvalidOperationException("User is not authenticated");
        }

        // Extract user ID from standard claim types (supports both "sub" and NameIdentifier)
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)
                          ?? httpContext.User.FindFirst("sub")
                          ?? throw new InvalidOperationException("User ID claim not found");

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new InvalidOperationException($"Invalid user ID format: {userIdClaim.Value}");
        }

        var permissions = GetClaimValues(httpContext, "permissions");
        var roles = GetClaimValues(httpContext, ClaimTypes.Role);

        return new CurrentUser(userId, permissions, roles);
    }

    private static IReadOnlyList<string> GetClaimValues(HttpContext httpContext, string claimType)
    {
        return httpContext.User.Claims
            .Where(claim => claim.Type == claimType)
            .Select(claim => claim.Value)
            .ToList();
    }
}
