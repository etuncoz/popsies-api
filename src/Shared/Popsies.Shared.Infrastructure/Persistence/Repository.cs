using Microsoft.EntityFrameworkCore;
using Popsies.Shared.Abstractions.Domain;
using Popsies.Shared.Abstractions.Persistence;

namespace Popsies.Shared.Infrastructure.Persistence;

/// <summary>
/// Base repository implementation using Entity Framework Core
/// </summary>
public abstract class Repository<TAggregate> : IRepository<TAggregate>
    where TAggregate : AggregateRoot
{
    protected readonly DbContext Context;
    protected readonly DbSet<TAggregate> DbSet;

    protected Repository(DbContext context)
    {
        Context = context;
        DbSet = context.Set<TAggregate>();
    }

    public virtual async Task<TAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(aggregate, cancellationToken);
    }

    public virtual void Update(TAggregate aggregate)
    {
        DbSet.Update(aggregate);
    }

    public virtual void Remove(TAggregate aggregate)
    {
        DbSet.Remove(aggregate);
    }
}
