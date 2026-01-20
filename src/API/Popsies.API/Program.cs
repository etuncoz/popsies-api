using Popsies.API.Exceptions;
using Popsies.Modules.Identity.Api;
using Popsies.Modules.Identity.Domain.Users.Events;
using Popsies.Modules.Identity.Domain.Guests.Events;
using Popsies.Modules.Quiz.Api;
using Popsies.Modules.Quiz.Domain.Quizzes.Events;
using Popsies.Modules.Quiz.Domain.Categories.Events;
using Popsies.Shared.Abstractions.Time;
using Popsies.Shared.Abstractions.Users;
using Popsies.Shared.Infrastructure.Events;
using Popsies.Shared.Infrastructure.Logging;
using Popsies.Shared.Infrastructure.Time;
using Popsies.Shared.Infrastructure.Users;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(3);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(3);
});

// Configure Serilog with centralized logging
Log.Logger = LoggingExtensions.ConfigureSerilog(builder.Configuration, "Popsies.API");

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add Problem Details support
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        // Add instance path
        context.ProblemDetails.Instance =
            $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

        // Add trace ID for debugging
        context.ProblemDetails.Extensions.TryAdd(
            "traceId", context.HttpContext.TraceIdentifier);
    };
});

// Register global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Add shared services
builder.Services.AddSingleton<IDateTime, DateTimeProvider>();

// Add HTTP context accessor and current user provider
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

// Add MassTransit with RabbitMQ for event bus
builder.Services.AddMassTransitWithRabbitMq(builder.Configuration, cfg =>
{
    // Register consumers for Identity domain events
    cfg.AddEventConsumer<UserRegisteredEvent>();
    cfg.AddEventConsumer<UserProfileUpdatedEvent>();
    cfg.AddEventConsumer<UserDeletedEvent>();
    cfg.AddEventConsumer<GuestCreatedEvent>();
    cfg.AddEventConsumer<GuestConvertedEvent>();
    cfg.AddEventConsumer<GuestExpiredEvent>();

    // Register consumers for Quiz domain events
    cfg.AddEventConsumer<QuizCreatedEvent>();
    cfg.AddEventConsumer<QuizUpdatedEvent>();
    cfg.AddEventConsumer<QuizPublishedEvent>();
    cfg.AddEventConsumer<QuizArchivedEvent>();
    cfg.AddEventConsumer<CategoryCreatedEvent>();
    cfg.AddEventConsumer<CategoryUpdatedEvent>();
    cfg.AddEventConsumer<CategoryDeactivatedEvent>();
});

// Configure JWT Authentication (Keycloak)
var keycloakSection = builder.Configuration.GetSection("Keycloak");
var realm = keycloakSection["Realm"] ?? throw new InvalidOperationException("Keycloak Realm is not configured");
var authServerUrl = keycloakSection["AuthServerUrl"] ?? throw new InvalidOperationException("Keycloak AuthServerUrl is not configured");
var clientId = keycloakSection["ClientId"] ?? throw new InvalidOperationException("Keycloak ClientId is not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"{authServerUrl}/realms/{realm}";
        options.Audience = clientId;
        options.RequireHttpsMetadata = false; // Development only - set to true in production
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = $"{authServerUrl}/realms/{realm}",
            ValidAudience = clientId,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add modules
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddQuizModule(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
// Add exception handler middleware (must be early in pipeline)
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply module configurations
app.UseIdentityModule();
app.UseQuizModule();

app.Run();
