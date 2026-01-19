using Popsies.Modules.Identity.Core.Domain.Entities;

namespace Popsies.Modules.Identity.Core.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByIdAsync(Guid tokenId, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefreshToken>> GetByGuestIdAsync(Guid guestId, CancellationToken cancellationToken = default);

    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    void Update(RefreshToken refreshToken);

    void Remove(RefreshToken refreshToken);

    Task<int> CountActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task RevokeAllByGuestIdAsync(Guid guestId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefreshToken>> GetExpiredTokensAsync(DateTime currentTime, CancellationToken cancellationToken = default);
}
