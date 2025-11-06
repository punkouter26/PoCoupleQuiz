using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace PoCoupleQuiz.Server.Extensions;

/// <summary>
/// Extension methods for configuring OpenTelemetry.
/// </summary>
public static class OpenTelemetryConfigurationExtensions
{
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
                else
                {
                    tracerProviderBuilder.AddConsoleExporter();
                }
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
                else
                {
                    meterProviderBuilder.AddConsoleExporter();
                }
            });

        return services;
    }
}
