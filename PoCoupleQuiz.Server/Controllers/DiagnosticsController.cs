using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace PoCoupleQuiz.Server.Controllers;

/// <summary>
/// Diagnostics controller for exposing configuration values (Development only).
/// Helps with debugging connection strings, API keys, and environment variables.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<DiagnosticsController> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Returns all configuration values with sensitive data partially masked.
    /// Only available in Development environment.
    /// </summary>
    [HttpGet]
    public IActionResult GetDiagnostics()
    {
        // Security: Only allow in Development
        if (!_environment.IsDevelopment())
        {
            _logger.LogWarning("Diagnostics endpoint accessed in non-Development environment: {Environment}", _environment.EnvironmentName);
            return StatusCode(403, new { error = "Diagnostics endpoint is only available in Development environment" });
        }

        _logger.LogInformation("Diagnostics endpoint accessed");

        var diagnostics = new
        {
            environment = _environment.EnvironmentName,
            timestamp = DateTime.UtcNow.ToString("o"),
            machineName = Environment.MachineName,
            applicationName = _environment.ApplicationName,
            contentRootPath = _environment.ContentRootPath,
            webRootPath = _environment.WebRootPath,
            configuration = GetConfigurationTree(),
            environmentVariables = GetMaskedEnvironmentVariables(),
            systemInfo = new
            {
                osVersion = Environment.OSVersion.ToString(),
                is64BitProcess = Environment.Is64BitProcess,
                processorCount = Environment.ProcessorCount,
                dotnetVersion = Environment.Version.ToString(),
                workingSet = $"{Environment.WorkingSet / 1024 / 1024} MB"
            }
        };

        return Ok(diagnostics);
    }

    /// <summary>
    /// Recursively builds configuration tree with masked sensitive values.
    /// </summary>
    private Dictionary<string, object> GetConfigurationTree()
    {
        var config = new Dictionary<string, object>();

        foreach (var section in _configuration.GetChildren())
        {
            config[section.Key] = BuildConfigNode(section);
        }

        return config;
    }

    /// <summary>
    /// Builds a node in the configuration tree, masking sensitive values.
    /// </summary>
    private object BuildConfigNode(IConfigurationSection section)
    {
        // If section has children, recurse
        if (section.GetChildren().Any())
        {
            var children = new Dictionary<string, object>();
            foreach (var child in section.GetChildren())
            {
                children[child.Key] = BuildConfigNode(child);
            }
            return children;
        }

        // Leaf node - mask if sensitive
        var value = section.Value;
        if (string.IsNullOrEmpty(value))
        {
            return "(empty)";
        }

        // Mask sensitive keys
        if (IsSensitiveKey(section.Path))
        {
            return MaskValue(value);
        }

        return value;
    }

    /// <summary>
    /// Determines if a configuration key contains sensitive data.
    /// </summary>
    private bool IsSensitiveKey(string key)
    {
        var sensitivePatterns = new[]
        {
            "password", "secret", "key", "token", "connectionstring",
            "apikey", "clientsecret", "connectionstrings", "credentials",
            "accountkey", "sastoken", "authorization"
        };

        var lowerKey = key.ToLowerInvariant();
        return sensitivePatterns.Any(pattern => lowerKey.Contains(pattern));
    }

    /// <summary>
    /// Masks a sensitive value by showing only first 4 and last 4 characters.
    /// </summary>
    private string MaskValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "(empty)";
        }

        if (value.Length <= 8)
        {
            return "****";
        }

        var first = value.Substring(0, 4);
        var last = value.Substring(value.Length - 4);
        var maskLength = Math.Min(value.Length - 8, 20); // Cap mask length
        var mask = new string('*', maskLength);

        return $"{first}{mask}{last}";
    }

    /// <summary>
    /// Gets environment variables with sensitive ones masked.
    /// </summary>
    private Dictionary<string, string> GetMaskedEnvironmentVariables()
    {
        var envVars = new Dictionary<string, string>();
        
        // Only include relevant environment variables (not all system vars)
        var relevantPrefixes = new[] { "ASPNETCORE_", "DOTNET_", "AZURE_", "KEYVAULT_", "APPLICATIONINSIGHTS_", "USE_" };

        foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            var key = entry.Key.ToString() ?? "";
            var value = entry.Value?.ToString() ?? "";

            // Filter to relevant variables only
            if (!relevantPrefixes.Any(prefix => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (IsSensitiveKey(key))
            {
                envVars[key] = MaskValue(value);
            }
            else
            {
                envVars[key] = value;
            }
        }

        return envVars;
    }

    /// <summary>
    /// Tests network connectivity to Azure services.
    /// </summary>
    [HttpGet("network")]
    public async Task<IActionResult> TestNetworkConnectivity()
    {
        if (!_environment.IsDevelopment())
        {
            return StatusCode(403, new { error = "Diagnostics endpoint is only available in Development environment" });
        }

        var results = new List<object>();

        // Test Azure Storage connectivity
        var storageConnectionString = _configuration["PoCoupleQuiz:AzureStorage:ConnectionString"];
        if (!string.IsNullOrEmpty(storageConnectionString))
        {
            try
            {
                var tableClient = new Azure.Data.Tables.TableServiceClient(storageConnectionString);
                await tableClient.GetPropertiesAsync();
                results.Add(new { service = "Azure Table Storage", status = "Connected", maskedConnectionString = MaskValue(storageConnectionString) });
            }
            catch (Exception ex)
            {
                results.Add(new { service = "Azure Table Storage", status = "Failed", error = ex.Message });
            }
        }

        // Test Key Vault connectivity
        var keyVaultUri = _configuration["KeyVault:VaultUri"];
        if (!string.IsNullOrEmpty(keyVaultUri))
        {
            try
            {
                var credential = new Azure.Identity.DefaultAzureCredential();
                var client = new Azure.Security.KeyVault.Secrets.SecretClient(new Uri(keyVaultUri), credential);
                // Just try to list (doesn't require actual secrets to exist)
                await foreach (var _ in client.GetPropertiesOfSecretsAsync().Take(1))
                {
                    break;
                }
                results.Add(new { service = "Azure Key Vault", status = "Connected", vaultUri = keyVaultUri });
            }
            catch (Exception ex)
            {
                results.Add(new { service = "Azure Key Vault", status = "Failed", error = ex.Message, vaultUri = keyVaultUri });
            }
        }

        return Ok(new
        {
            timestamp = DateTime.UtcNow.ToString("o"),
            connectivityTests = results
        });
    }
}
