using Microsoft.Extensions.Logging;

namespace PoCoupleQuiz.Core.Services;

/// <summary>
/// Service for checking server connectivity.
/// </summary>
public interface IConnectivityCheckService
{
    /// <summary>
    /// Checks if the server is reachable.
    /// </summary>
    Task<bool> CheckServerConnectivityAsync(string serverUrl);
}

public class ConnectivityCheckService : IConnectivityCheckService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ConnectivityCheckService> _logger;

    public ConnectivityCheckService(HttpClient httpClient, ILogger<ConnectivityCheckService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> CheckServerConnectivityAsync(string serverUrl)
    {
        try
        {
            var response = await _httpClient.GetAsync(serverUrl);
            var isSuccess = response.IsSuccessStatusCode;
            
            if (isSuccess)
            {
                _logger.LogInformation("Server connectivity check successful for {ServerUrl}", serverUrl);
            }
            else
            {
                _logger.LogWarning("Server connectivity check failed for {ServerUrl} with status code {StatusCode}", 
                    serverUrl, response.StatusCode);
            }
            
            return isSuccess;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Server connectivity check failed for {ServerUrl}", serverUrl);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during server connectivity check for {ServerUrl}", serverUrl);
            return false;
        }
    }
}
