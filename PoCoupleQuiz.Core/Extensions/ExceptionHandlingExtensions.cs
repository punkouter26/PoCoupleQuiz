using Microsoft.Extensions.Logging;

namespace PoCoupleQuiz.Core.Extensions;

/// <summary>
/// Extension methods for consistent exception handling with logging.
/// </summary>
public static class ExceptionHandlingExtensions
{
    /// <summary>
    /// Executes an async operation with automatic exception logging.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operationName">A descriptive name for the operation.</param>
    /// <param name="defaultValue">The default value to return on error.</param>
    /// <returns>The result of the operation, or the default value if an exception occurs.</returns>
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
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operationName">A descriptive name for the operation.</param>
    /// <param name="onError">Optional callback to execute when an exception occurs.</param>
    /// <returns>The result of the operation, or default if an exception occurs.</returns>
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
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operationName">A descriptive name for the operation.</param>
    /// <param name="defaultValue">The default value to return on error.</param>
    /// <returns>The result of the operation, or the default value if an exception occurs.</returns>
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
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operationName">A descriptive name for the operation.</param>
    /// <returns>True if successful, false if an exception occurred.</returns>
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
}
