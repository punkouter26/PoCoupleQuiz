using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PoCoupleQuiz.Client;
using PoCoupleQuiz.Client.Extensions;
using PoCoupleQuiz.Client.Services;
using Radzen;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddRadzenComponents();
builder.Services.AddFluentUIComponents();
builder.Services.AddClientServices();

// Add browser diagnostics service
builder.Services.AddScoped<BrowserDiagnosticsService>();

// Add Application Insights telemetry service for client-side tracking
builder.Services.AddScoped<ApplicationInsightsTelemetryService>();

var app = builder.Build();

// Initialize Application Insights in background (don't block app startup)
_ = Task.Run(async () =>
{
    try
    {
        // Get App Insights connection string from server config
        var httpClient = app.Services.GetRequiredService<HttpClient>();
        var response = await httpClient.GetAsync("api/config/appinsights");
        
        if (response.IsSuccessStatusCode)
        {
            var connectionString = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var telemetryService = app.Services.GetRequiredService<ApplicationInsightsTelemetryService>();
                await telemetryService.InitializeAsync(connectionString.Trim('"'));
                Console.WriteLine("[Startup] Application Insights telemetry initialized");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Failed to initialize Application Insights: {ex.Message}");
    }
});

// Initialize browser diagnostics in background (don't block app startup)
_ = Task.Run(async () =>
{
    try
    {
        var diagnosticsService = app.Services.GetRequiredService<BrowserDiagnosticsService>();
        await diagnosticsService.InitializeAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to initialize browser diagnostics: {ex.Message}");
    }
});

await app.RunAsync();
