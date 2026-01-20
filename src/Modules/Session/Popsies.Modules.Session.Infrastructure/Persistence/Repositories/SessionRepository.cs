using Microsoft.EntityFrameworkCore;
using Popsies.Modules.Session.Application.Common.Repositories;

namespace Popsies.Modules.Session.Infrastructure.Persistence.Repositories;

internal sealed class SessionRepository(SessionDbContext context) : ISessionRepository
{
    private readonly SessionDbContext _context = context;

    public async Task<Domain.Sessions.Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _context.Sessions
            .Include(s => s.Players)
            .Include(s => s.Answers)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<Domain.Sessions.Session?> GetBySessionCodeAsync(string sessionCode, CancellationToken cancellationToken)
        => await _context.Sessions
            .Include(s => s.Players)
            .Include(s => s.Answers)
            .FirstOrDefaultAsync(s => s.SessionCode == sessionCode, cancellationToken);

    public async Task<bool> ExistsBySessionCodeAsync(string sessionCode, CancellationToken cancellationToken)
        => await _context.Sessions
            .AnyAsync(s => s.SessionCode == sessionCode, cancellationToken);

    public async Task AddAsync(Domain.Sessions.Session session, CancellationToken cancellationToken)
        => await _context.Sessions.AddAsync(session, cancellationToken);

    public void Update(Domain.Sessions.Session session)
        => _context.Sessions.Update(session);

    public void Remove(Domain.Sessions.Session session)
        => _context.Sessions.Remove(session);
}
