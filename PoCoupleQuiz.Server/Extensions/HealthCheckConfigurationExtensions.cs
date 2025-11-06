using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PoCoupleQuiz.Server.HealthChecks;

namespace PoCoupleQuiz.Server.Extensions;

/// <summary>
/// Extension methods for configuring health checks.
/// </summary>
public static class HealthCheckConfigurationExtensions
{
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
}
