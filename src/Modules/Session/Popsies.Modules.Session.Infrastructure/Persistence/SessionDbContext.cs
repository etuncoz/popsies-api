using Microsoft.EntityFrameworkCore;
using Popsies.Modules.Session.Domain.Answers;
using Popsies.Modules.Session.Domain.Players;

namespace Popsies.Modules.Session.Infrastructure.Persistence;

/// <summary>
/// Database context for Session module
/// </summary>
public sealed class SessionDbContext : DbContext
{
    public DbSet<Domain.Sessions.Session> Sessions { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Answer> Answers { get; set; }

    public SessionDbContext(DbContextOptions<SessionDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("session");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SessionDbContext).Assembly);
    }
}
