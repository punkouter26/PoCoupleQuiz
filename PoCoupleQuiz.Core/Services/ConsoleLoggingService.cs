using Microsoft.Extensions.Logging;

namespace PoCoupleQuiz.Core.Services;

/// <summary>
/// Service for handling console logging with different log levels.
/// </summary>
public interface IConsoleLoggingService
{
    /// <summary>
    /// Logs a message to the console with the specified level.
    /// </summary>
    void LogMessage(string level, string message, string? category = null);
}

public class ConsoleLoggingService : IConsoleLoggingService
{
    private readonly ILogger<ConsoleLoggingService> _logger;

    public ConsoleLoggingService(ILogger<ConsoleLoggingService> logger)
    {
        _logger = logger;
    }

    public void LogMessage(string level, string message, string? category = null)
    {
        var logMessage = string.IsNullOrEmpty(category) 
            ? message 
            : $"[{category}] {message}";

        switch (level.ToLowerInvariant())
        {
            case "info":
            case "information":
                _logger.LogInformation("{Message}", logMessage);
                break;
            case "warn":
            case "warning":
                _logger.LogWarning("{Message}", logMessage);
                break;
            case "error":
                _logger.LogError("{Message}", logMessage);
                break;
            case "debug":
                _logger.LogDebug("{Message}", logMessage);
                break;
            default:
                _logger.LogInformation("{Message}", logMessage);
                break;
        }
    }
}
