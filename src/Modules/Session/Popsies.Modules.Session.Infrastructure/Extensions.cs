using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Popsies.Modules.Session.Application.Common.Repositories;
using Popsies.Modules.Session.Infrastructure.Persistence;
using Popsies.Modules.Session.Infrastructure.Persistence.Repositories;

namespace Popsies.Modules.Session.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddSessionInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<SessionDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("SessionDb")));

        // Repositories
        services.AddScoped<ISessionRepository, SessionRepository>();

        return services;
    }
}
