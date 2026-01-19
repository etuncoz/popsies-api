using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Popsies.Shared.Abstractions.Events;

namespace Popsies.Shared.Infrastructure.Events;

/// <summary>
/// In-memory event bus implementation for cross-module communication
/// </summary>
public sealed class InMemoryEventBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InMemoryEventBus> _logger;

    public InMemoryEventBus(IServiceProvider serviceProvider, ILogger<InMemoryEventBus> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        _logger.LogInformation("Publishing event {EventType} with ID {EventId}",
            typeof(TEvent).Name, @event.EventId);

        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();

        var tasks = handlers.Select(handler =>
            HandleEventAsync(handler, @event, cancellationToken));

        await Task.WhenAll(tasks);
    }

    public async Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        var eventsList = events.ToList();

        _logger.LogInformation("Publishing {Count} events", eventsList.Count);

        foreach (var @event in eventsList)
        {
            var eventType = @event.GetType();
            var method = typeof(InMemoryEventBus)
                .GetMethod(nameof(PublishAsync), new[] { eventType, typeof(CancellationToken) })
                ?.MakeGenericMethod(eventType);

            if (method is not null)
            {
                var task = (Task?)method.Invoke(this, new object[] { @event, cancellationToken });
                if (task is not null)
                {
                    await task;
                }
            }
        }
    }

    private async Task HandleEventAsync<TEvent>(
        IEventHandler<TEvent> handler,
        TEvent @event,
        CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        try
        {
            await handler.HandleAsync(@event, cancellationToken);

            _logger.LogInformation("Event {EventType} handled successfully by {HandlerType}",
                typeof(TEvent).Name, handler.GetType().Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling event {EventType} with handler {HandlerType}",
                typeof(TEvent).Name, handler.GetType().Name);
            throw;
        }
    }
}
