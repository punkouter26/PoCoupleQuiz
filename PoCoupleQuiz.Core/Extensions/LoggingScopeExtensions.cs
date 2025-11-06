using Microsoft.Extensions.Logging;

namespace PoCoupleQuiz.Core.Extensions;

/// <summary>
/// Extension methods for creating logging scopes with structured properties.
/// </summary>
public static class LoggingScopeExtensions
{
    /// <summary>
    /// Creates a logging scope with structured properties.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="properties">Property key-value pairs to include in the scope.</param>
    /// <returns>A disposable scope object.</returns>
    public static IDisposable BeginScopeWithProperties(
        this ILogger logger,
        params (string Key, object? Value)[] properties)
    {
        var dict = properties.ToDictionary(p => p.Key, p => p.Value);
        return logger.BeginScope(dict);
    }

    /// <summary>
    /// Creates a logging scope from a dictionary of properties.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="properties">Dictionary of properties.</param>
    /// <returns>A disposable scope object.</returns>
    public static IDisposable BeginScopeWithDictionary(
        this ILogger logger,
        Dictionary<string, object?> properties)
    {
        return logger.BeginScope(properties);
    }
}
