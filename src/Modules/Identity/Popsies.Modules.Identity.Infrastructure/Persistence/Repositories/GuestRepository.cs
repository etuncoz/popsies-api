using Microsoft.EntityFrameworkCore;
using Popsies.Modules.Identity.Application.Common.Repositories;
using Popsies.Modules.Identity.Domain.Guests;

namespace Popsies.Modules.Identity.Infrastructure.Persistence.Repositories;

internal sealed class GuestRepository(IdentityDbContext context) : IGuestRepository
{
    private readonly IdentityDbContext _dbContext = context;

    public async Task<Guest?> GetByIdAsync(Guid guestId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Guests.FindAsync(new object[] { guestId }, cancellationToken);
    }

    public async Task AddAsync(Guest guest, CancellationToken cancellationToken = default)
    {
        await _dbContext.Guests.AddAsync(guest, cancellationToken);
    }

    public void Update(Guest guest)
    {
        _dbContext.Guests.Update(guest);
    }

    public async Task<IReadOnlyList<Guest>> GetExpiredGuestsAsync(DateTime currentTime, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Guests
            .Where(g => !g.IsExpired && g.ExpiresAt <= currentTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guest>> GetGuestsWithActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Guests
            .Where(g => g.ActiveQuizSessionId != null)
            .ToListAsync(cancellationToken);
    }
}
