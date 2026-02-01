using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoCoupleQuiz.Server.HealthChecks;

/// <summary>
/// Startup health check that waits for Azure Table Storage to be ready.
/// Uses retry logic to handle cold-start scenarios with Azurite or Azure.
/// Tagged with "live" so the app reports healthy for liveness probes immediately,
/// but "ready" status waits for storage connectivity.
/// </summary>
public class StartupHealthCheck : IHealthCheck
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly ILogger<StartupHealthCheck> _logger;
    private static volatile bool _isReady = false;
    private static readonly object _lock = new();
    private static DateTime _lastCheck = DateTime.MinValue;
    private static readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5);

    public StartupHealthCheck(
        TableServiceClient tableServiceClient,
        ILogger<StartupHealthCheck> logger)
    {
        _tableServiceClient = tableServiceClient;
        _logger = logger;
    }

    /// <summary>
    /// Returns true if the startup warmup has completed successfully.
    /// </summary>
    public static bool IsReady => _isReady;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // If already marked ready, return immediately
        if (_isReady)
        {
            return HealthCheckResult.Healthy("Application startup complete");
        }

        // Throttle checks to avoid hammering the storage
        lock (_lock)
        {
            if (DateTime.UtcNow - _lastCheck < _checkInterval)
            {
                return HealthCheckResult.Degraded("Startup warmup in progress...");
            }
            _lastCheck = DateTime.UtcNow;
        }

        try
        {
            _logger.LogInformation("Startup health check: verifying Azure Table Storage connectivity...");
            
            // Attempt to connect to Table Storage with retry
            var connected = await TryConnectWithRetryAsync(cancellationToken);
            
            if (connected)
            {
                _isReady = true;
                _logger.LogInformation("Startup health check: Azure Table Storage is ready");
                return HealthCheckResult.Healthy("Application startup complete - storage connected");
            }
            
            return HealthCheckResult.Degraded("Waiting for Azure Table Storage to become available...");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Degraded("Startup check cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Startup health check: Storage not yet available");
            return HealthCheckResult.Degraded($"Waiting for dependencies: {ex.Message}");
        }
    }

    private async Task<bool> TryConnectWithRetryAsync(CancellationToken cancellationToken)
    {
        const int maxRetries = 3;
        const int delayMs = 500;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                // Try to list tables - this verifies connectivity
                var tables = _tableServiceClient.QueryAsync(cancellationToken: cancellationToken);
                await foreach (var _ in tables.ConfigureAwait(false))
                {
                    // Successfully connected if we can enumerate any table
                    return true;
                }
                // No tables exist but connection succeeded
                return true;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogDebug(ex, "Startup retry {Attempt}/{MaxRetries} failed, waiting {Delay}ms", 
                    attempt, maxRetries, delayMs * attempt);
                await Task.Delay(delayMs * attempt, cancellationToken);
            }
        }

        return false;
    }

    /// <summary>
    /// Resets the ready state. Useful for testing.
    /// </summary>
    internal static void Reset()
    {
        _isReady = false;
        _lastCheck = DateTime.MinValue;
    }
}
