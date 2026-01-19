using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Popsies.Modules.Identity.Core.Handlers;
using Popsies.Modules.Identity.Core.Repositories;
using Popsies.Modules.Identity.Core.Services;
using Popsies.Modules.Identity.Infrastructure.Persistence;
using Popsies.Modules.Identity.Infrastructure.Persistence.Repositories;
using Popsies.Modules.Identity.Infrastructure.Services;
using Popsies.Shared.Abstractions.Commands;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Infrastructure.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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

        // Command Handlers
        services.AddScoped<ICommandHandler<Core.Commands.RegisterUserCommand, Guid>, RegisterUserCommandHandler>();
        services.AddScoped<ICommandHandler<Core.Commands.LoginCommand, Core.Commands.LoginResult>, LoginCommandHandler>();
        services.AddScoped<ICommandHandler<Core.Commands.RefreshTokenCommand, Core.Commands.TokenRefreshResult>, RefreshTokenCommandHandler>();
        services.AddScoped<ICommandHandler<Core.Commands.CreateGuestCommand, Core.Commands.GuestCreationResult>, CreateGuestCommandHandler>();

        // Command Dispatcher
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();

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
