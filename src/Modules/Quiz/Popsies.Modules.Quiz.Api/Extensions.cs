using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Modules.Quiz.Application.UseCases.CreateQuiz;
using Popsies.Modules.Quiz.Infrastructure.Persistence;
using Popsies.Modules.Quiz.Infrastructure.Persistence.Repositories;
using Popsies.Shared.Abstractions.Persistence;

namespace Popsies.Modules.Quiz.Api;

/// <summary>
/// Extension methods for registering Quiz module dependencies
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Adds the Quiz module services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddQuizModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<QuizDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("QuizDb"),
                npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(120); // 2 minutes
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                }));

        // Repositories
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // MediatR (auto-registers all IRequestHandler implementations)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Extensions).Assembly);
        });

        // FluentValidation validators
        services.AddValidatorsFromAssembly(typeof(Extensions).Assembly);

        // Unit of Work
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<QuizDbContext>());

        return services;
    }

    /// <summary>
    /// Configures the Quiz module middleware and applies database migrations
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseQuizModule(this IApplicationBuilder app)
    {
        // Apply migrations automatically
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<QuizDbContext>();
        dbContext.Database.Migrate();

        return app;
    }
}
