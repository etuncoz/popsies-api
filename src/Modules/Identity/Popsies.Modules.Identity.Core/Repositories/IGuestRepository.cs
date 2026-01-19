using Popsies.Modules.Identity.Core.Domain.Entities;

namespace Popsies.Modules.Identity.Core.Repositories;

public interface IGuestRepository
{
    Task<Guest?> GetByIdAsync(Guid guestId, CancellationToken cancellationToken = default);

    Task AddAsync(Guest guest, CancellationToken cancellationToken = default);

    void Update(Guest guest);

    Task<IReadOnlyList<Guest>> GetExpiredGuestsAsync(DateTime currentTime, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guest>> GetGuestsWithActiveSessionsAsync(CancellationToken cancellationToken = default);
}
