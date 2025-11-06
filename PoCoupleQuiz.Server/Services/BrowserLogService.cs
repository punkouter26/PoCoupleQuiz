using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace PoCoupleQuiz.Server.Services;

/// <summary>
/// Service for writing browser logs to files.
/// </summary>
public interface IBrowserLogService
{
    /// <summary>
    /// Writes a log message to the browser log file.
    /// </summary>
    Task WriteToBrowserLogFileAsync(string message, string logType);
}

public class BrowserLogService : IBrowserLogService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<BrowserLogService> _logger;

    public BrowserLogService(IWebHostEnvironment environment, ILogger<BrowserLogService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task WriteToBrowserLogFileAsync(string message, string logType)
    {
        try
        {
            var logDirectory = Path.Combine(_environment.ContentRootPath, "DEBUG");
            
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            var logFilePath = Path.Combine(logDirectory, "browser.log");
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] [{logType}] {message}{Environment.NewLine}";

            await File.AppendAllTextAsync(logFilePath, logEntry);
            
            _logger.LogDebug("Browser log written to file: {LogType} - {Message}", logType, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing to browser log file");
        }
    }
}
