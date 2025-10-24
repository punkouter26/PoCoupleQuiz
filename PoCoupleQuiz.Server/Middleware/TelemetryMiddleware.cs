using System.Diagnostics;

namespace PoCoupleQuiz.Server.Middleware;

/// <summary>
/// Middleware for capturing custom telemetry and performance metrics.
/// Enriches logs with request duration, response size, and custom events.
/// </summary>
public class TelemetryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TelemetryMiddleware> _logger;

    public TelemetryMiddleware(RequestDelegate next, ILogger<TelemetryMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            stopwatch.Stop();

            // Capture response metrics
            var responseSize = responseBody.Length;
            var duration = stopwatch.ElapsedMilliseconds;

            // Log performance telemetry with structured properties
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["RequestPath"] = context.Request.Path,
                ["RequestMethod"] = context.Request.Method,
                ["StatusCode"] = context.Response.StatusCode,
                ["DurationMs"] = duration,
                ["ResponseSizeBytes"] = responseSize,
                ["UserAgent"] = context.Request.Headers["User-Agent"].ToString(),
                ["RemoteIP"] = context.Connection.RemoteIpAddress?.ToString()
            }))
            {
                if (context.Response.StatusCode >= 400)
                {
                    _logger.LogWarning(
                        "Request completed with error: {Method} {Path} -> {StatusCode} ({Duration}ms)",
                        context.Request.Method,
                        context.Request.Path,
                        context.Response.StatusCode,
                        duration);
                }
                else if (duration > 1000)
                {
                    _logger.LogWarning(
                        "Slow request detected: {Method} {Path} took {Duration}ms",
                        context.Request.Method,
                        context.Request.Path,
                        duration);
                }
                else
                {
                    _logger.LogInformation(
                        "Request completed: {Method} {Path} -> {StatusCode} ({Duration}ms, {Size} bytes)",
                        context.Request.Method,
                        context.Request.Path,
                        context.Response.StatusCode,
                        duration,
                        responseSize);
                }
            }

            // Copy response back to original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "Unhandled exception in request pipeline: {Method} {Path} (after {Duration}ms)",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}

/// <summary>
/// Extension methods for registering telemetry middleware.
/// </summary>
public static class TelemetryMiddlewareExtensions
{
    public static IApplicationBuilder UseTelemetryMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TelemetryMiddleware>();
    }
}
