using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Popsies.Shared.Abstractions.Caching;
using Popsies.Shared.Abstractions.Events;
using Popsies.Shared.Abstractions.Time;
using Popsies.Shared.Infrastructure.Behaviors;
using Popsies.Shared.Infrastructure.Caching;
using Popsies.Shared.Infrastructure.Events;
using Popsies.Shared.Infrastructure.Time;

namespace Popsies.Shared.Infrastructure;

/// <summary>
/// Extension methods for registering shared infrastructure services
/// </summary>
public static class Extensions
{
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Time provider
        services.AddSingleton<IDateTime, DateTimeProvider>();

        // Event bus
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        // MediatR pipeline behaviors (used by all modules)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

        // Redis cache
        var redisConnection = configuration["Redis:ConnectionString"] ?? "localhost:6379";
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "Popsies_";
        });

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
