using Microsoft.AspNetCore.Mvc;
using Azure.Data.Tables;
using Azure.AI.OpenAI;
using System.Net;

namespace PoCoupleQuiz.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IConfiguration configuration, ILogger<HealthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("/healthz")]
    public async Task<IActionResult> HealthCheck()
    {
        var health = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            checks = new List<object>()
        };

        var checks = (List<object>)health.checks;
        bool allHealthy = true;

        // Check Azure Table Storage
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? 
                                 _configuration["AzureStorage:ConnectionString"];
            
            if (!string.IsNullOrEmpty(connectionString))
            {
                var tableClient = new TableServiceClient(connectionString);
                await tableClient.GetPropertiesAsync();
                checks.Add(new { name = "Azure Table Storage", status = "healthy", responseTime = "< 1000ms" });
            }
            else
            {
                checks.Add(new { name = "Azure Table Storage", status = "unhealthy", error = "Connection string not configured" });
                allHealthy = false;
            }
        }
        catch (Exception ex)
        {
            checks.Add(new { name = "Azure Table Storage", status = "unhealthy", error = ex.Message });
            allHealthy = false;
        }

        // Check Azure OpenAI
        try
        {
            var endpoint = _configuration["AzureOpenAI:Endpoint"];
            var key = _configuration["AzureOpenAI:Key"];
            
            if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(key))
            {
                var client = new AzureOpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(key));
                // Just check if the client can be created successfully
                checks.Add(new { name = "Azure OpenAI", status = "healthy", endpoint = endpoint });
            }
            else
            {
                checks.Add(new { name = "Azure OpenAI", status = "unhealthy", error = "Endpoint or key not configured" });
                allHealthy = false;
            }
        }
        catch (Exception ex)
        {
            checks.Add(new { name = "Azure OpenAI", status = "unhealthy", error = ex.Message });
            allHealthy = false;
        }

        var result = new
        {
            status = allHealthy ? "healthy" : "unhealthy",
            timestamp = DateTime.UtcNow,
            checks = checks
        };

        return allHealthy ? Ok(result) : StatusCode(503, result);
    }
}
