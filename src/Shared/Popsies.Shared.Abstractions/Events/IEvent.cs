namespace Popsies.Shared.Abstractions.Events;

/// <summary>
/// Marker interface for domain events
/// </summary>
public interface IEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}

/// <summary>
/// Base implementation for domain events
/// </summary>
public abstract record DomainEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
