using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace PoCoupleQuiz.Server.Controllers;

/// <summary>
/// API endpoint for receiving client-side logs from the Blazor WebAssembly application.
/// Enables centralized logging and telemetry collection from browser-based clients.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LogController : ControllerBase
{
    private readonly ILogger<LogController> _logger;

    public LogController(ILogger<LogController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Receives and logs messages from the client application.
    /// POST /api/log/client
    /// </summary>
    /// <param name="request">Client log entry with level, message, and optional properties</param>
    /// <returns>204 No Content on success</returns>
    [HttpPost("client")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult LogClientMessage([FromBody] ClientLogRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validate log level
        if (!Enum.TryParse<LogLevel>(request.Level, true, out var logLevel))
        {
            return BadRequest($"Invalid log level: {request.Level}");
        }

        // Create structured log with client context
        using (_logger.BeginScope(new Dictionary<string, object?>
        {
            ["ClientSource"] = "BlazorWASM",
            ["ClientUrl"] = request.Url,
            ["ClientUserAgent"] = Request.Headers["User-Agent"].ToString(),
            ["ClientTimestamp"] = request.Timestamp,
            ["ClientProperties"] = request.Properties
        }))
        {
            _logger.Log(logLevel, "[CLIENT] {Message}", request.Message);
        }

        return NoContent();
    }
}

/// <summary>
/// Request model for client-side logging.
/// </summary>
public class ClientLogRequest
{
    /// <summary>
    /// Log level: Trace, Debug, Information, Warning, Error, Critical
    /// </summary>
    [Required]
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Log message text
    /// </summary>
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Client URL where the log was generated
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Client-side timestamp in ISO 8601 format
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional structured properties for the log entry
    /// </summary>
    public Dictionary<string, object?>? Properties { get; set; }
}
