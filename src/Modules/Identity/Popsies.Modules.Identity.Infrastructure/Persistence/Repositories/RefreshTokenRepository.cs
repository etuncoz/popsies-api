using Microsoft.EntityFrameworkCore;
using Popsies.Modules.Identity.Application.Common.Repositories;
using Popsies.Modules.Identity.Domain.RefreshTokens;

namespace Popsies.Modules.Identity.Infrastructure.Persistence.Repositories;

internal sealed class RefreshTokenRepository(IdentityDbContext context) : IRefreshTokenRepository
{
    private readonly IdentityDbContext _dbContext = context;

    public async Task<RefreshToken?> GetByIdAsync(Guid tokenId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens.FindAsync(new object[] { tokenId }, cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetByGuestIdAsync(Guid guestId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .Where(rt => rt.GuestId == guestId)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public void Update(RefreshToken refreshToken)
    {
        _dbContext.RefreshTokens.Update(refreshToken);
    }

    public void Remove(RefreshToken refreshToken)
    {
        _dbContext.RefreshTokens.Remove(refreshToken);
    }

    public async Task<int> CountActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .CountAsync(rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsExpired, cancellationToken);
    }

    public async Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke();
        }
    }

    public async Task RevokeAllByGuestIdAsync(Guid guestId, CancellationToken cancellationToken = default)
    {
        var tokens = await _dbContext.RefreshTokens
            .Where(rt => rt.GuestId == guestId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke();
        }
    }

    public async Task<IReadOnlyList<RefreshToken>> GetExpiredTokensAsync(DateTime currentTime, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .Where(rt => !rt.IsExpired && rt.ExpiresAt <= currentTime)
            .ToListAsync(cancellationToken);
    }
}
