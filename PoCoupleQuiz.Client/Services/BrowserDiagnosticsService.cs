using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;

namespace PoCoupleQuiz.Client.Services;

/// <summary>
/// Service for capturing browser console logs and network activity
/// Implements Observer pattern for real-time logging
/// </summary>
public class BrowserDiagnosticsService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;
    private IJSObjectReference? _diagnosticsModule;
    private DotNetObjectReference<BrowserDiagnosticsService>? _dotNetObjectRef;

    public BrowserDiagnosticsService(IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _diagnosticsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/diagnostics.js");
            _dotNetObjectRef = DotNetObjectReference.Create(this);
            await _diagnosticsModule.InvokeVoidAsync("initializeDiagnostics", _dotNetObjectRef);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize browser diagnostics: {ex.Message}");
        }
    }

    [JSInvokable]
    public async Task LogConsoleMessage(string level, string message, string timestamp, string? stack = null)
    {
        var logEntry = new
        {
            timestamp,
            source = "browser-console",
            level,
            message,
            stack,
            url = await GetCurrentUrl()
        };

        SendToServer("/api/diagnostics/console", logEntry);
    }

    [JSInvokable]
    public async Task LogNetworkActivity(string method, string url, int status, double duration, string requestId)
    {
        var logEntry = new
        {
            timestamp = DateTime.UtcNow.ToString("O"),
            source = "browser-network",
            method,
            url,
            status,
            duration,
            requestId,
            currentPage = await GetCurrentUrl()
        };

        SendToServer("/api/diagnostics/network", logEntry);
    }

    [JSInvokable]
    public async Task LogUserAction(string action, string elementType, string? elementId = null, string? data = null)
    {
        var logEntry = new
        {
            timestamp = DateTime.UtcNow.ToString("O"),
            source = "user-action",
            action,
            elementType,
            elementId,
            data,
            url = await GetCurrentUrl()
        };

        SendToServer("/api/diagnostics/user-action", logEntry);
    }

    private async Task<string> GetCurrentUrl()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("eval", "window.location.href");
        }
        catch
        {
            return "unknown";
        }
    }

    private void SendToServer(string endpoint, object logEntry)
    {
        try
        {
            var json = JsonSerializer.Serialize(logEntry);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Fire and forget - don't wait for response to avoid blocking UI
            _ = Task.Run(async () =>
            {
                try
                {
                    await _httpClient.PostAsync(endpoint, content);
                }
                catch
                {
                    // Silently ignore network errors for diagnostic logging
                }
            });
        }
        catch
        {
            // Silently ignore serialization errors for diagnostic logging
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_diagnosticsModule != null)
            {
                await _diagnosticsModule.InvokeVoidAsync("cleanup");
                await _diagnosticsModule.DisposeAsync();
            }
            _dotNetObjectRef?.Dispose();
        }
        catch
        {
            // Ignore disposal errors
        }
    }
}
