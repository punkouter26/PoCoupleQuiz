using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace PoCoupleQuiz.Server.Extensions;

/// <summary>
/// Extension methods for configuring Serilog logging.
/// </summary>
public static class SerilogConfigurationExtensions
{
    /// <summary>
    /// Adds and configures Serilog for the application.
    /// </summary>
    public static IHostBuilder AddSerilogConfiguration(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "PoCoupleQuiz")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.Debug();

            // Add Application Insights in production
            if (context.HostingEnvironment.IsProduction())
            {
                var appInsightsConnectionString = context.Configuration["ApplicationInsights:ConnectionString"];
                if (!string.IsNullOrEmpty(appInsightsConnectionString))
                {
                    configuration.WriteTo.ApplicationInsights(
                        services.GetRequiredService<Microsoft.ApplicationInsights.TelemetryClient>(),
                        TelemetryConverter.Traces,
                        LogEventLevel.Information);
                }
            }
        });
    }
}
