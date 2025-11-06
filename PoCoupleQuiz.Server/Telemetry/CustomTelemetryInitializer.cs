using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;

namespace PoCoupleQuiz.Server.Telemetry
{
    /// <summary>
    /// Telemetry initializer that enriches all Application Insights telemetry with custom properties.
    /// Adds contextual information like user role, game ID, correlation ID, and environment details.
    /// Implements ITelemetryInitializer to intercept and modify telemetry before it's sent to Azure.
    /// </summary>
    public class CustomTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            // Add custom properties to all telemetry
            if (telemetry is ISupportProperties telemetryWithProperties)
            {
                var properties = telemetryWithProperties.Properties;

                // Application-level properties
                properties["Application.Name"] = "Po.CoupleQuiz";
                properties["Application.Version"] = "1.0.0";
                properties["Environment.Name"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

                // W3C Trace Context correlation ID (from Activity)
                var activity = Activity.Current;
                if (activity != null)
                {
                    properties["CorrelationId"] = activity.TraceId.ToString();
                    properties["ParentId"] = activity.ParentId ?? "None";
                    properties["SpanId"] = activity.SpanId.ToString();

                    // Add custom baggage from Activity (if any)
                    foreach (var baggage in activity.Baggage)
                    {
                        properties[$"Baggage.{baggage.Key}"] = baggage.Value;
                    }
                }

                // HTTP request context (if available)
                if (httpContext != null)
                {
                    // Request details
                    properties["Request.Method"] = httpContext.Request.Method;
                    properties["Request.Path"] = httpContext.Request.Path.Value ?? "/";
                    properties["Request.QueryString"] = httpContext.Request.QueryString.Value ?? "";

                    // Client information
                    properties["Client.IP"] = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                    properties["Client.UserAgent"] = httpContext.Request.Headers["User-Agent"].ToString();

                    // Custom headers (game context)
                    if (httpContext.Request.Headers.TryGetValue("X-Game-Id", out var gameId))
                    {
                        properties["Game.Id"] = gameId.ToString();
                    }

                    if (httpContext.Request.Headers.TryGetValue("X-Player-Id", out var playerId))
                    {
                        properties["Player.Id"] = playerId.ToString();
                    }

                    if (httpContext.Request.Headers.TryGetValue("X-Player-Role", out var playerRole))
                    {
                        properties["Player.Role"] = playerRole.ToString();
                    }

                    // Query parameters that might indicate user context
                    if (httpContext.Request.Query.TryGetValue("difficulty", out var difficulty))
                    {
                        properties["Game.Difficulty"] = difficulty.ToString();
                    }

                    // Session information
                    if (httpContext.Session != null && httpContext.Session.IsAvailable)
                    {
                        properties["Session.Id"] = httpContext.Session.Id;
                    }
                }

                // Machine/deployment information
                properties["Machine.Name"] = Environment.MachineName;
                properties["OS.Platform"] = Environment.OSVersion.Platform.ToString();
                properties["Runtime.Version"] = Environment.Version.ToString();
            }
        }
    }
}
