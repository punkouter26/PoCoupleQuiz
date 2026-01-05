using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PoCoupleQuiz.Server.HealthChecks;
using Serilog;
using Serilog.Events;

namespace PoCoupleQuiz.Server.Extensions;

/// <summary>
/// Extension methods for configuring application services.
/// </summary>
public static class ConfigurationExtensions
{
    #region Health Checks

    /// <summary>
    /// Adds and configures health checks for the application.
    /// </summary>
    public static IServiceCollection AddHealthCheckConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck<AzureTableStorageHealthCheck>(
                "azure_table_storage",
                tags: new[] { "ready", "storage" })
            .AddCheck<AzureOpenAIHealthCheck>(
                "azure_openai",
                tags: new[] { "ready", "ai" });

        return services;
    }

    #endregion

    #region OpenTelemetry

    /// <summary>
    /// Adds and configures OpenTelemetry for the application.
    /// </summary>
    public static IServiceCollection AddOpenTelemetryConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isProduction)
    {
        var resourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService("PoCoupleQuiz");

        services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("PoCoupleQuiz.Server")
                    .AddSource("PoCoupleQuiz.Core");

                if (isProduction)
                {
                    var appInsightsConnectionString = configuration["ApplicationInsights:ConnectionString"];
                    if (!string.IsNullOrEmpty(appInsightsConnectionString))
                    {
                        tracerProviderBuilder.AddAzureMonitorTraceExporter(options =>
                        {
                            options.ConnectionString = appInsightsConnectionString;
                        });
                    }
                }
                // Console exporter removed - too verbose for development
            })
            .WithMetrics(meterProviderBuilder =>
            {
                meterProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter("PoCoupleQuiz.Server")
                    .AddMeter("PoCoupleQuiz.Core");

                if (isProduction)
                {
                    var appInsightsConnectionString = configuration["ApplicationInsights:ConnectionString"];
                    if (!string.IsNullOrEmpty(appInsightsConnectionString))
                    {
                        meterProviderBuilder.AddAzureMonitorMetricExporter(options =>
                        {
                            options.ConnectionString = appInsightsConnectionString;
                        });
                    }
                }
                // Console exporter removed - too verbose for development
            });

        return services;
    }

    #endregion

    #region Serilog

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

    #endregion
}
