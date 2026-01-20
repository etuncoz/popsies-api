using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Popsies.Modules.Session.Application.UseCases.CreateSession;
using Popsies.Modules.Session.Infrastructure;
using Popsies.Modules.Session.Infrastructure.Persistence;

namespace Popsies.Modules.Session.Api;

public static class Extensions
{
    public static IServiceCollection AddSessionModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Infrastructure layer
        services.AddSessionInfrastructure(configuration);

        // MediatR (auto-registers all IRequestHandler implementations)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateSessionCommand).Assembly);
        });

        // FluentValidation validators
        services.AddValidatorsFromAssembly(typeof(CreateSessionCommandValidator).Assembly);

        return services;
    }

    public static IApplicationBuilder UseSessionModule(this IApplicationBuilder app)
    {
        // Run migrations
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SessionDbContext>();
        dbContext.Database.Migrate();

        return app;
    }
}
