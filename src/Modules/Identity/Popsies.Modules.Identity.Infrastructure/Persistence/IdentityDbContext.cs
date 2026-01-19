using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Popsies.Modules.Identity.Core.Domain.Aggregates;
using Popsies.Modules.Identity.Core.Domain.Entities;
using Popsies.Shared.Abstractions.Domain;
using Popsies.Shared.Abstractions.Events;
using Popsies.Shared.Abstractions.Persistence;

namespace Popsies.Modules.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext : DbContext, IUnitOfWork
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<IdentityDbContext> _logger;

    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public IdentityDbContext(
        DbContextOptions<IdentityDbContext> options,
        IEventBus eventBus,
        ILogger<IdentityDbContext> logger)
        : base(options)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker.Entries<Entity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .SelectMany(e => e.DomainEvents)
            .ToList();

        int result;
        try
        {
            result = await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // Log detailed information about the failed entities
            LogDatabaseUpdateError(ex);
            throw;
        }
        catch (PostgresException ex)
        {
            // Log PostgreSQL-specific errors
            LogPostgresError(ex);
            throw;
        }
        catch (OperationCanceledException ex)
        {
            // Log timeout/cancellation details
            _logger.LogError(ex,
                "Database operation was cancelled or timed out. Pending changes: {PendingChanges}",
                GetPendingChangesDescription());
            throw;
        }

        // Publish domain events after successful save
        foreach (var domainEvent in domainEvents)
        {
            await _eventBus.PublishAsync(domainEvent, cancellationToken);
        }

        // Clear domain events
        foreach (var entity in ChangeTracker.Entries<Entity>().Select(e => e.Entity))
        {
            entity.ClearDomainEvents();
        }

        return result;
    }

    private void LogDatabaseUpdateError(DbUpdateException ex)
    {
        _logger.LogError(ex, "Database update failed: {Message}", ex.Message);

        foreach (var entry in ex.Entries)
        {
            var entityType = entry.Entity.GetType().Name;
            var entityState = entry.State;

            _logger.LogError(
                "Failed entity: {EntityType}, State: {State}, Entity: {@Entity}",
                entityType,
                entityState,
                entry.Entity);

            // Log property values
            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;
                var currentValue = property.CurrentValue;
                var originalValue = property.OriginalValue;

                if (currentValue is string stringValue && stringValue?.Length > 100)
                {
                    _logger.LogError(
                        "Property: {Property}, Length: {Length}, MaxLength: {MaxLength}",
                        propertyName,
                        stringValue.Length,
                        property.Metadata.GetMaxLength());
                }
                else
                {
                    _logger.LogError(
                        "Property: {Property}, Current: {Current}, Original: {Original}",
                        propertyName,
                        currentValue,
                        originalValue);
                }
            }
        }

        // Check for inner PostgreSQL exception
        if (ex.InnerException is PostgresException pgEx)
        {
            LogPostgresError(pgEx);
        }
    }

    private void LogPostgresError(PostgresException ex)
    {
        _logger.LogError(ex,
            "PostgreSQL Error - Code: {SqlState}, Severity: {Severity}, Message: {Message}, Detail: {Detail}, Hint: {Hint}, Table: {Table}, Column: {Column}, Constraint: {Constraint}",
            ex.SqlState,
            ex.Severity,
            ex.MessageText,
            ex.Detail,
            ex.Hint,
            ex.TableName,
            ex.ColumnName,
            ex.ConstraintName);
    }

    private string GetPendingChangesDescription()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged)
            .Select(e => $"{e.Entity.GetType().Name} ({e.State})")
            .ToList();

        return entries.Any()
            ? string.Join(", ", entries)
            : "No pending changes";
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.RollbackTransactionAsync(cancellationToken);
    }
}
