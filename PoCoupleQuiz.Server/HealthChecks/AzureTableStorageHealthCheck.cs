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
    private readonly TableServiceClient _tableServiceClient;

    public AzureTableStorageHealthCheck(TableServiceClient tableServiceClient)
    {
        _tableServiceClient = tableServiceClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Use Aspire-injected TableServiceClient (works with Azurite or Azure Storage)
            await _tableServiceClient.GetPropertiesAsync(cancellationToken);
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
