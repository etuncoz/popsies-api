using SessionAggregate = Popsies.Modules.Session.Domain.Sessions.Session;

namespace Popsies.Modules.Session.Application.Common.Repositories;

/// <summary>
/// Repository interface for Session aggregate
/// </summary>
public interface ISessionRepository
{
    Task<SessionAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<SessionAggregate?> GetBySessionCodeAsync(string sessionCode, CancellationToken cancellationToken);
    Task<bool> ExistsBySessionCodeAsync(string sessionCode, CancellationToken cancellationToken);
    Task AddAsync(SessionAggregate session, CancellationToken cancellationToken);
    void Update(SessionAggregate session);
    void Remove(SessionAggregate session);
}
