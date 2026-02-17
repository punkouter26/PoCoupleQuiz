using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PoCoupleQuiz.Server.HealthChecks;

/// <summary>
/// Health check for Azure Key Vault connectivity.
/// Verifies that the application can successfully communicate with the configured Key Vault.
/// </summary>
public class KeyVaultHealthCheck : IHealthCheck
{
    private readonly SecretClient? _secretClient;
    private readonly ILogger<KeyVaultHealthCheck> _logger;
    private readonly string _vaultUri;

    public KeyVaultHealthCheck(
        IConfiguration configuration,
        ILogger<KeyVaultHealthCheck> logger)
    {
        _logger = logger;
        _vaultUri = configuration["KeyVault:VaultUri"] 
            ?? configuration["AZURE_KEY_VAULT_URI"] 
            ?? "https://kv-poshared.vault.azure.net/";

        // Only create SecretClient if Key Vault is not skipped
        var skipKeyVault = configuration["SKIP_KEYVAULT"]?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
        if (!skipKeyVault)
        {
            try
            {
                var vaultUri = new Uri(_vaultUri);
                _secretClient = new SecretClient(vaultUri, new Azure.Identity.DefaultAzureCredential());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create SecretClient for Key Vault: {VaultUri}", _vaultUri);
            }
        }
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // If SecretClient is not configured, treat as degraded (not healthy, but not unhealthy)
            if (_secretClient == null)
            {
                return HealthCheckResult.Degraded(
                    $"Key Vault client not configured (SKIP_KEYVAULT=true or configuration missing). Vault URI: {_vaultUri}");
            }

            // Attempt to list secret properties (doesn't retrieve actual secret values)
            // This verifies network connectivity and authentication
            var secretsPage = _secretClient.GetPropertiesOfSecretsAsync(cancellationToken);
            
            // Just check if we can enumerate - don't need to iterate all secrets
            await foreach (var _ in secretsPage.WithCancellation(cancellationToken))
            {
                // Successfully connected and authenticated
                return HealthCheckResult.Healthy(
                    $"Successfully connected to Key Vault: {_vaultUri}");
            }

            // No secrets found, but connection successful
            return HealthCheckResult.Healthy(
                $"Connected to Key Vault (no secrets found): {_vaultUri}");
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 403)
        {
            // Authentication/Authorization failure
            _logger.LogWarning(ex, "Key Vault access denied: {VaultUri}", _vaultUri);
            return HealthCheckResult.Unhealthy(
                $"Access denied to Key Vault: {_vaultUri}. Check Managed Identity permissions.",
                ex);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            // Key Vault not found
            _logger.LogWarning(ex, "Key Vault not found: {VaultUri}", _vaultUri);
            return HealthCheckResult.Unhealthy(
                $"Key Vault not found: {_vaultUri}",
                ex);
        }
        catch (Exception ex)
        {
            // Network or other connectivity issue
            _logger.LogError(ex, "Key Vault health check failed: {VaultUri}", _vaultUri);
            return HealthCheckResult.Unhealthy(
                $"Failed to connect to Key Vault: {_vaultUri}",
                ex);
        }
    }
}
