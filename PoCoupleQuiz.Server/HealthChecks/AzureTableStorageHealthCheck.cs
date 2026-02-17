using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace PoCoupleQuiz.Server.HealthChecks;

/// <summary>
/// Health check for Azure Table Storage connectivity.
/// Uses Aspire-injected TableServiceClient for connection.
/// </summary>
public class AzureTableStorageHealthCheck : IHealthCheck
{
    private readonly TableServiceClient? _tableServiceClient;

    public AzureTableStorageHealthCheck(TableServiceClient? tableServiceClient = null)
    {
        _tableServiceClient = tableServiceClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // If no TableServiceClient is registered, we're using in-memory storage
        if (_tableServiceClient == null)
        {
            return HealthCheckResult.Healthy("Using in-memory storage (Azure Table Storage not configured)");
        }

        try
        {
            // List tables to verify connectivity using data-plane operations only
            // This works with Storage Table Data Contributor role (no service-level permissions needed)
            var tables = _tableServiceClient.QueryAsync(cancellationToken: cancellationToken);
            int tableCount = 0;
            await foreach (var _ in tables.ConfigureAwait(false))
            {
                tableCount++;
                if (tableCount >= 1) break; // Only need to verify we can list at least one table
            }
            return HealthCheckResult.Healthy("Connected to Azure Table Storage");
        }
        catch (Exception ex)
        {
            // Return Degraded instead of Unhealthy so service can still start
            return HealthCheckResult.Degraded(
                "Azure Table Storage is unreachable - service may have limited functionality",
                ex);
        }
    }
}