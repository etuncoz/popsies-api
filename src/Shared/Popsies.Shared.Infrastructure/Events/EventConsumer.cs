using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Popsies.Shared.Abstractions.Events;

namespace Popsies.Shared.Infrastructure.Events;

/// <summary>
/// Generic MassTransit consumer that delegates to IEventHandler implementations
/// </summary>
public sealed class EventConsumer<TEvent> : IConsumer<TEvent>
    where TEvent : class, IEvent
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventConsumer<TEvent>> _logger;

    public EventConsumer(
        IServiceProvider serviceProvider,
        ILogger<EventConsumer<TEvent>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Consuming event {EventType} with ID {EventId}",
            typeof(TEvent).Name,
            @event.EventId);

        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();

        var tasks = handlers.Select(handler =>
            HandleEventAsync(handler, @event, context.CancellationToken));

        await Task.WhenAll(tasks);

        _logger.LogInformation(
            "Event {EventType} with ID {EventId} consumed successfully by {HandlerCount} handler(s)",
            typeof(TEvent).Name,
            @event.EventId,
            handlers.Count());
    }

    private async Task HandleEventAsync(
        IEventHandler<TEvent> handler,
        TEvent @event,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleAsync(@event, cancellationToken);

            _logger.LogInformation(
                "Event {EventType} handled successfully by {HandlerType}",
                typeof(TEvent).Name,
                handler.GetType().Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling event {EventType} with handler {HandlerType}",
                typeof(TEvent).Name,
                handler.GetType().Name);
            throw;
        }
    }
}
