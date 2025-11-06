using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoCoupleQuiz.Client.Services
{
    /// <summary>
    /// Client-side Application Insights telemetry service.
    /// Provides strongly-typed wrapper around JavaScript SDK for W3C Trace Context correlation.
    /// </summary>
    public class ApplicationInsightsTelemetryService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private IJSObjectReference? _telemetryModule;
        private bool _isInitialized;

        public ApplicationInsightsTelemetryService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> InitializeAsync(string connectionString)
        {
            if (_isInitialized)
                return true;

            try
            {
                _telemetryModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/telemetry.js");
                _isInitialized = await _telemetryModule.InvokeAsync<bool>("initializeAppInsights", connectionString);

                if (_isInitialized)
                {
                    Console.WriteLine("[Telemetry] Application Insights initialized successfully");
                }
                else
                {
                    Console.WriteLine("[Telemetry] Application Insights initialization failed");
                }

                return _isInitialized;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Telemetry] Failed to initialize Application Insights: {ex.Message}");
                return false;
            }
        }

        public async Task TrackEventAsync(string name, Dictionary<string, string>? properties = null, Dictionary<string, double>? measurements = null)
        {
            if (!_isInitialized || _telemetryModule == null)
                return;

            try
            {
                await _telemetryModule.InvokeVoidAsync("trackEvent", name, properties, measurements);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Telemetry] Failed to track event: {ex.Message}");
            }
        }

        public async Task TrackMetricAsync(string name, double value, Dictionary<string, string>? properties = null)
        {
            if (!_isInitialized || _telemetryModule == null)
                return;

            try
            {
                await _telemetryModule.InvokeVoidAsync("trackMetric", name, value, properties);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Telemetry] Failed to track metric: {ex.Message}");
            }
        }

        public async Task TrackExceptionAsync(Exception exception, int severityLevel = 3, Dictionary<string, string>? properties = null)
        {
            if (!_isInitialized || _telemetryModule == null)
                return;

            try
            {
                var error = new
                {
                    message = exception.Message,
                    stack = exception.StackTrace,
                    type = exception.GetType().Name
                };

                await _telemetryModule.InvokeVoidAsync("trackException", error, severityLevel, properties);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Telemetry] Failed to track exception: {ex.Message}");
            }
        }

        public async Task TrackPageViewAsync(string? name = null, string? url = null, Dictionary<string, string>? properties = null)
        {
            if (!_isInitialized || _telemetryModule == null)
                return;

            try
            {
                await _telemetryModule.InvokeVoidAsync("trackPageView", name, url, properties);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Telemetry] Failed to track page view: {ex.Message}");
            }
        }

        public async Task FlushAsync()
        {
            if (!_isInitialized || _telemetryModule == null)
                return;

            try
            {
                await _telemetryModule.InvokeVoidAsync("flush");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Telemetry] Failed to flush telemetry: {ex.Message}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_telemetryModule != null)
            {
                await FlushAsync();
                await _telemetryModule.DisposeAsync();
            }
        }
    }
}
