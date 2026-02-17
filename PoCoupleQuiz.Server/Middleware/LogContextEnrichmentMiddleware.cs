using Serilog.Context;

namespace PoCoupleQuiz.Server.Middleware;

/// <summary>
/// Middleware to enrich all logs with user and session context for better traceability.
/// Adds UserId and SessionId to all log entries automatically.
/// </summary>
public class LogContextEnrichmentMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LogContextEnrichmentMiddleware> _logger;

    public LogContextEnrichmentMiddleware(RequestDelegate next, ILogger<LogContextEnrichmentMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get or create session ID
        var sessionId = context.Session.Id;
        if (string.IsNullOrEmpty(sessionId))
        {
            // Session not yet established, force it to be created
            await context.Session.LoadAsync();
            sessionId = context.Session.Id;
        }

        // Get user identity
        var userId = context.User?.Identity?.Name ?? "Anonymous";
        
        // Get IP address for additional context
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        // Enrich all logs in this request with user and session context
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("SessionId", sessionId))
        using (LogContext.PushProperty("IpAddress", ipAddress))
        {
            _logger.LogDebug("Request started with UserId={UserId}, SessionId={SessionId}", userId, sessionId);
            
            await _next(context);
        }
    }
}

/// <summary>
/// Extension methods to easily add LogContextEnrichmentMiddleware to the pipeline.
/// </summary>
public static class LogContextEnrichmentMiddlewareExtensions
{
    public static IApplicationBuilder UseLogContextEnrichment(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LogContextEnrichmentMiddleware>();
    }
}
