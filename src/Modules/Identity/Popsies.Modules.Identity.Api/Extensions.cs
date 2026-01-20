using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Popsies.Modules.Identity.Application.Common.Repositories;
using Popsies.Modules.Identity.Application.Common.Services;
using Popsies.Modules.Identity.Application.UseCases.Register;
using Popsies.Modules.Identity.Infrastructure.Persistence;
using Popsies.Modules.Identity.Infrastructure.Persistence.Repositories;
using Popsies.Modules.Identity.Infrastructure.Services;
using Popsies.Shared.Abstractions.Persistence;

namespace Popsies.Modules.Identity.Api;

public static class Extensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("IdentityDb"),
                npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(120); // 2 minutes
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                }));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IGuestRepository, GuestRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Services
        services.AddScoped<IUsernameGenerator, UsernameGenerator>();
        services.AddScoped<IGuestTokenGenerator, GuestTokenGenerator>();

        // HttpClient Factory (required by KeycloakService)
        services.AddHttpClient();

        // Keycloak Service
        services.AddScoped<IKeycloakService, KeycloakService>();

        // MediatR (auto-registers all IRequestHandler implementations)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Extensions).Assembly);
        });

        // FluentValidation validators
        services.AddValidatorsFromAssembly(typeof(Extensions).Assembly);

        // Unit of Work
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IdentityDbContext>());

        return services;
    }

    public static IApplicationBuilder UseIdentityModule(this IApplicationBuilder app)
    {
        // Apply migrations automatically
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        dbContext.Database.Migrate();

        return app;
    }
}
