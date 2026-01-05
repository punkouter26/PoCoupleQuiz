using Microsoft.Extensions.Logging;

namespace PoCoupleQuiz.Core.Extensions;

/// <summary>
/// Extension methods for consistent exception handling and logging scopes.
/// </summary>
public static class LoggingExtensions
{
    #region Exception Handling

    /// <summary>
    /// Executes an async operation with automatic exception logging.
    /// </summary>
    public static async Task<T?> ExecuteWithLoggingAsync<T>(
        this Task<T> operation,
        ILogger logger,
        string operationName,
        T? defaultValue = default)
    {
        try
        {
            return await operation;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing {Operation}", operationName);
            return defaultValue;
        }
    }

    /// <summary>
    /// Executes an async operation with automatic exception logging and custom exception handling.
    /// </summary>
    public static async Task<T?> ExecuteWithLoggingAsync<T>(
        this Task<T> operation,
        ILogger logger,
        string operationName,
        Func<Exception, T?> onError)
    {
        try
        {
            return await operation;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing {Operation}", operationName);
            return onError(ex);
        }
    }

    /// <summary>
    /// Executes a synchronous operation with automatic exception logging.
    /// </summary>
    public static T? ExecuteWithLogging<T>(
        this Func<T> operation,
        ILogger logger,
        string operationName,
        T? defaultValue = default)
    {
        try
        {
            return operation();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing {Operation}", operationName);
            return defaultValue;
        }
    }

    /// <summary>
    /// Executes an async operation without a return value with automatic exception logging.
    /// </summary>
    public static async Task<bool> TryExecuteAsync(
        this Func<Task> operation,
        ILogger logger,
        string operationName)
    {
        try
        {
            await operation();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing {Operation}", operationName);
            return false;
        }
    }

    #endregion

    #region Logging Scopes

    /// <summary>
    /// Creates a logging scope with structured properties.
    /// </summary>
    public static IDisposable? BeginScopeWithProperties(
        this ILogger logger,
        params (string Key, object? Value)[] properties)
    {
        var dict = properties.ToDictionary(p => p.Key, p => p.Value);
        return logger.BeginScope(dict);
    }

    /// <summary>
    /// Creates a logging scope from a dictionary of properties.
    /// </summary>
    public static IDisposable? BeginScopeWithDictionary(
        this ILogger logger,
        Dictionary<string, object?> properties)
    {
        return logger.BeginScope(properties);
    }

    #endregion
}
