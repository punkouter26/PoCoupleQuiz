using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Azure;

namespace PoCoupleQuiz.Server.HealthChecks;

/// <summary>
/// Health check for Azure OpenAI connectivity.
/// Validates endpoint and API key configuration.
/// </summary>
public class AzureOpenAIHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public AzureOpenAIHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = _configuration["AzureOpenAI:Endpoint"];
            var key = _configuration["AzureOpenAI:Key"];
            var deploymentName = _configuration["AzureOpenAI:DeploymentName"];

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
            {
                return HealthCheckResult.Degraded("Azure OpenAI is not configured - using mock service");
            }

            if (endpoint.Contains("your-resource-name") || key.Contains("your-") || string.IsNullOrEmpty(deploymentName))
            {
                return HealthCheckResult.Degraded("Azure OpenAI configuration contains placeholder values - using mock service");
            }

            // Test connection by creating client
            var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));

            // Note: We don't make an actual API call to avoid costs and rate limits
            // Just verify the client can be instantiated with the provided credentials
            return HealthCheckResult.Healthy($"Azure OpenAI configured with deployment: {deploymentName}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded(
                "Azure OpenAI configuration is invalid - using mock service",
                ex);
        }
    }
}
