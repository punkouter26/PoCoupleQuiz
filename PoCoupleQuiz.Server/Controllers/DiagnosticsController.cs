using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace PoCoupleQuiz.Server.Controllers
{
    /// <summary>
    /// Controller for handling diagnostic operations including connectivity checks and client-side logging
    /// Implements Facade pattern to centralize diagnostic data handling
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DiagnosticsController> _logger;
        private readonly string _debugPath;

        public DiagnosticsController(HttpClient httpClient, ILogger<DiagnosticsController> logger, IWebHostEnvironment env)
        {
            _httpClient = httpClient;
            _logger = logger;
            _debugPath = Path.Combine(env.ContentRootPath, "DEBUG");

            // Ensure DEBUG directory exists
            if (!Directory.Exists(_debugPath))
                Directory.CreateDirectory(_debugPath);
        }

        [HttpGet("internet")]
        public async Task<IActionResult> CheckInternetConnection()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://www.microsoft.com/");
                response.EnsureSuccessStatusCode();
                return Ok(true);
            }
            catch (HttpRequestException)
            {
                return Ok(false);
            }
        }

        [HttpPost("console")]
        public async Task<IActionResult> LogConsoleMessage([FromBody] JsonElement logData)
        {
            try
            {
                var level = logData.GetProperty("level").GetString() ?? "info";
                var message = logData.GetProperty("message").GetString() ?? "";
                var timestamp = logData.GetProperty("timestamp").GetString() ?? DateTime.UtcNow.ToString("O");
                var url = logData.TryGetProperty("url", out var urlProp) ? urlProp.GetString() ?? "" : "";
                var stack = logData.TryGetProperty("stack", out var stackProp) ? stackProp.GetString() : null;

                // Log to server logger with appropriate level
                var logMessage = "CLIENT CONSOLE [{Level}] {Url}: {Message}";
                var logArgs = new object[] { level.ToUpper(), url, message };

                switch (level.ToLower())
                {
                    case "error":
                        _logger.LogError("CLIENT CONSOLE [ERROR] {Url}: {Message} {Stack}", url, message, stack ?? "");
                        break;
                    case "warn":
                        _logger.LogWarning(logMessage, logArgs);
                        break;
                    case "debug":
                        _logger.LogDebug(logMessage, logArgs);
                        break;
                    default:
                        _logger.LogInformation(logMessage, logArgs);
                        break;
                }

                // Also write to dedicated browser console log file
                await WriteToBrowserLogFile("console", logData);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process console log from client");
                return StatusCode(500);
            }
        }

        [HttpPost("network")]
        public async Task<IActionResult> LogNetworkActivity([FromBody] JsonElement logData)
        {
            // TEMPORARILY DISABLED: Network activity logging disabled to reduce log noise
            // This endpoint was causing cascading diagnostic requests flooding the console
            return Ok();

            /*
            try
            {
                var method = logData.GetProperty("method").GetString() ?? "";
                var url = logData.GetProperty("url").GetString() ?? "";
                var status = logData.GetProperty("status").GetInt32();
                var duration = logData.GetProperty("duration").GetDouble();
                var requestId = logData.GetProperty("requestId").GetString() ?? "";

                // Only log to file, not to console to reduce noise
                // _logger.LogInformation("CLIENT NETWORK {Method} {Url} -> {Status} ({Duration:F2}ms) [{RequestId}]", 
                //     method, url, status, duration, requestId);

                // Write to dedicated network activity log file only
                await WriteToBrowserLogFile("network", logData);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process network activity log from client");
                return StatusCode(500);
            }
            */
        }

        [HttpPost("user-action")]
        public async Task<IActionResult> LogUserAction([FromBody] JsonElement logData)
        {
            try
            {
                var action = logData.GetProperty("action").GetString() ?? "";
                var elementType = logData.GetProperty("elementType").GetString() ?? "";
                var elementId = logData.TryGetProperty("elementId", out var idProp) ? idProp.GetString() : null;
                var url = logData.GetProperty("url").GetString() ?? "";

                _logger.LogInformation("CLIENT ACTION {Action} on {ElementType} {ElementId} at {Url}",
                    action, elementType, elementId ?? "unnamed", url);

                // Write to dedicated user action log file
                await WriteToBrowserLogFile("user-actions", logData);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process user action log from client");
                return StatusCode(500);
            }
        }

        private async Task WriteToBrowserLogFile(string logType, JsonElement logData)
        {
            try
            {
                var fileName = $"browser-{logType}.log";
                var filePath = Path.Combine(_debugPath, fileName);

                var logEntry = JsonSerializer.Serialize(logData, new JsonSerializerOptions
                {
                    WriteIndented = false
                }) + Environment.NewLine;

                // Append to file (create if doesn't exist)
                await System.IO.File.AppendAllTextAsync(filePath, logEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write browser log to file for type: {LogType}", logType);
            }
        }

        [HttpDelete("clear")]
        public IActionResult ClearDiagnosticFiles()
        {
            try
            {
                var files = new[] { "browser-console.log", "browser-network.log", "browser-user-actions.log" };

                foreach (var file in files)
                {
                    var filePath = Path.Combine(_debugPath, file);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _logger.LogInformation("Diagnostic files cleared successfully");
                return Ok(new { message = "Diagnostic files cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear diagnostic files");
                return StatusCode(500, new { error = "Failed to clear diagnostic files" });
            }
        }

        [HttpGet("status")]
        public IActionResult GetDiagnosticStatus()
        {
            try
            {
                var files = new[] { "browser-console.log", "browser-network.log", "browser-user-actions.log", "log.txt" };
                var status = new Dictionary<string, object>();

                foreach (var file in files)
                {
                    var filePath = Path.Combine(_debugPath, file);
                    if (System.IO.File.Exists(filePath))
                    {
                        var fileInfo = new FileInfo(filePath);
                        status[file] = new
                        {
                            exists = true,
                            size = fileInfo.Length,
                            lastModified = fileInfo.LastWriteTime.ToString("O")
                        };
                    }
                    else
                    {
                        status[file] = new { exists = false };
                    }
                }

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get diagnostic status");
                return StatusCode(500);
            }
        }
    }
}
