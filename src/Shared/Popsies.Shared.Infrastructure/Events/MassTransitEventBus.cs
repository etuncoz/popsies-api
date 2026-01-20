using MassTransit;
using Microsoft.Extensions.Logging;
using Popsies.Shared.Abstractions.Events;

namespace Popsies.Shared.Infrastructure.Events;

/// <summary>
/// MassTransit-based event bus implementation using RabbitMQ
/// </summary>
public sealed class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MassTransitEventBus> _logger;

    public MassTransitEventBus(
        IPublishEndpoint publishEndpoint,
        ILogger<MassTransitEventBus> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        try
        {
            _logger.LogInformation(
                "Publishing event {EventType} with ID {EventId} to message broker",
                typeof(TEvent).Name,
                @event.EventId);

            await _publishEndpoint.Publish(@event, cancellationToken);

            _logger.LogInformation(
                "Event {EventType} with ID {EventId} published successfully",
                typeof(TEvent).Name,
                @event.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error publishing event {EventType} with ID {EventId}",
                typeof(TEvent).Name,
                @event.EventId);
            throw;
        }
    }

    public async Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        var eventsList = events.ToList();

        _logger.LogInformation("Publishing {Count} events to message broker", eventsList.Count);

        foreach (var @event in eventsList)
        {
            var eventType = @event.GetType();
            var publishMethod = typeof(MassTransitEventBus)
                .GetMethod(nameof(PublishAsync), new[] { eventType, typeof(CancellationToken) })
                ?.MakeGenericMethod(eventType);

            if (publishMethod is not null)
            {
                var task = (Task?)publishMethod.Invoke(this, new object[] { @event, cancellationToken });
                if (task is not null)
                {
                    await task;
                }
            }
        }

        _logger.LogInformation("All {Count} events published successfully", eventsList.Count);
    }
}
