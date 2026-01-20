using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Popsies.Shared.Abstractions.Events;

namespace Popsies.Shared.Infrastructure.Events;

/// <summary>
/// Extension methods for configuring MassTransit with RabbitMQ
/// </summary>
public static class MassTransitExtensions
{
    public static IServiceCollection AddMassTransitWithRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        var rabbitMqConfig = configuration.GetSection("RabbitMQ");
        var host = rabbitMqConfig["Host"] ?? "localhost";
        var virtualHost = rabbitMqConfig["VirtualHost"] ?? "/";
        var username = rabbitMqConfig["Username"] ?? "guest";
        var password = rabbitMqConfig["Password"] ?? "guest";

        services.AddMassTransit(configurator =>
        {
            // Register consumers
            configureConsumers?.Invoke(configurator);

            configurator.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(host, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // Configure message retry
                cfg.UseMessageRetry(r => r.Incremental(
                    retryLimit: 3,
                    initialInterval: TimeSpan.FromSeconds(1),
                    intervalIncrement: TimeSpan.FromSeconds(2)));

                cfg.UseCircuitBreaker(r =>
                {
                    r.TrackingPeriod = TimeSpan.FromMinutes(1);
                    r.TripThreshold = 15;
                    r.ActiveThreshold = 10;
                    r.ResetInterval = TimeSpan.FromMinutes(15);
                });

                // Configure endpoints
                cfg.ConfigureEndpoints(context);
            });
        });

        // Register the event bus implementation
        services.AddScoped<IEventBus, MassTransitEventBus>();

        return services;
    }

    /// <summary>
    /// Register a consumer for a specific event type
    /// </summary>
    public static void AddEventConsumer<TEvent>(this IBusRegistrationConfigurator configurator)
        where TEvent : class, IEvent
    {
        configurator.AddConsumer<EventConsumer<TEvent>>();
    }
}
