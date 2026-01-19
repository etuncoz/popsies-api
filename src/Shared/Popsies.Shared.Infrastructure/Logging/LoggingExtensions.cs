using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace Popsies.Shared.Infrastructure.Logging;

/// <summary>
/// Extensions for configuring Serilog logging
/// </summary>
public static class LoggingExtensions
{
    public static ILogger ConfigureSerilog(IConfiguration configuration, string applicationName)
    {
        var elasticsearchUri = configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";

        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName)
            .Enrich.WithProperty("Environment", environment)
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUri))
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv8,
                IndexFormat = $"{applicationName.ToLower()}-logs-{environment.ToLower()}-{{0:yyyy.MM.dd}}",
                NumberOfShards = 2,
                NumberOfReplicas = 1,
                MinimumLogEventLevel = LogEventLevel.Information,
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
                FailureCallback = (e, ex) => Console.WriteLine($"Unable to submit event: {e.MessageTemplate}. Exception: {ex?.Message}"),
                TypeName = null // Required for Elasticsearch 8.x
            })
            .CreateLogger();
    }
}
