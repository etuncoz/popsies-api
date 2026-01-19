namespace Popsies.Shared.Abstractions.Events;

/// <summary>
/// Event bus for publishing domain events across modules
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent;

    Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default);
}
