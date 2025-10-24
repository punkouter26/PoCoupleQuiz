using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace PoCoupleQuiz.Server.HealthChecks;

/// <summary>
/// Health check for Azure Table Storage connectivity.
/// Validates connection string and attempts to query service.
/// </summary>
public class AzureTableStorageHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public AzureTableStorageHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = _configuration["AzureStorage:ConnectionString"];

            if (string.IsNullOrEmpty(connectionString))
            {
                return HealthCheckResult.Degraded("Azure Storage connection string is not configured");
            }

            // For Azurite (local development)
            if (connectionString.Contains("UseDevelopmentStorage=true") || 
                connectionString.Contains("127.0.0.1:10002"))
            {
                var tableServiceClient = new TableServiceClient(connectionString);
                await tableServiceClient.GetPropertiesAsync(cancellationToken);
                return HealthCheckResult.Healthy("Connected to Azurite (local development storage)");
            }

            // For Azure Table Storage (production)
            var serviceClient = new TableServiceClient(connectionString);
            await serviceClient.GetPropertiesAsync(cancellationToken);
            return HealthCheckResult.Healthy("Connected to Azure Table Storage");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Azure Table Storage is unreachable",
                ex);
        }
    }
}
