using Popsies.Modules.Identity.Api;
using Popsies.Shared.Abstractions.Events;
using Popsies.Shared.Abstractions.Time;
using Popsies.Shared.Infrastructure.Events;
using Popsies.Shared.Infrastructure.Time;
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

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add shared services
builder.Services.AddSingleton<IDateTime, DateTimeProvider>();
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

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

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply module configurations
app.UseIdentityModule();

app.Run();
