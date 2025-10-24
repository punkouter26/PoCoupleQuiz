using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace PoCoupleQuiz.Client.Services;

/// <summary>
/// Client-side logger that sends logs to the server for centralized collection.
/// Implements ILogger interface to integrate with .NET logging infrastructure.
/// </summary>
public class ServerLogger : ILogger
{
    private readonly HttpClient _httpClient;
    private readonly string _categoryName;

    public ServerLogger(HttpClient httpClient, string categoryName)
    {
        _httpClient = httpClient;
        _categoryName = categoryName;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        // Only send Warning and above to server to reduce noise
        return logLevel >= LogLevel.Warning;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        if (exception != null)
        {
            message += $"\nException: {exception.Message}\nStackTrace: {exception.StackTrace}";
        }

        // Fire and forget - don't block on logging
        _ = SendLogToServerAsync(logLevel, message);
    }

    private async Task SendLogToServerAsync(LogLevel logLevel, string message)
    {
        try
        {
            var logRequest = new
            {
                Level = logLevel.ToString(),
                Message = $"[{_categoryName}] {message}",
                Url = GetCurrentUrl(),
                Timestamp = DateTime.UtcNow,
                Properties = new Dictionary<string, object?>
                {
                    ["Category"] = _categoryName,
                    ["Source"] = "BlazorClient"
                }
            };

            await _httpClient.PostAsJsonAsync("/api/log/client", logRequest);
        }
        catch
        {
            // Silently fail - don't crash the app if logging fails
            // Could log to browser console here as fallback
        }
    }

    private static string GetCurrentUrl()
    {
        try
        {
            // In Blazor WASM, window.location.href is available via JSInterop
            // For simplicity, return a placeholder - can be enhanced with JSInterop
            return "blazor-client";
        }
        catch
        {
            return "unknown";
        }
    }
}

/// <summary>
/// Logger provider for ServerLogger.
/// </summary>
public class ServerLoggerProvider : ILoggerProvider
{
    private readonly HttpClient _httpClient;

    public ServerLoggerProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new ServerLogger(_httpClient, categoryName);
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}

/// <summary>
/// Extension methods for registering server logging.
/// </summary>
public static class ServerLoggingExtensions
{
    public static ILoggingBuilder AddServerLogging(this ILoggingBuilder builder, HttpClient httpClient)
    {
        builder.AddProvider(new ServerLoggerProvider(httpClient));
        return builder;
    }
}
