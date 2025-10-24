using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PoCoupleQuiz.Client;
using PoCoupleQuiz.Client.Extensions;
using PoCoupleQuiz.Client.Services;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddRadzenComponents();
builder.Services.AddClientServices();

// Add browser diagnostics service
builder.Services.AddScoped<BrowserDiagnosticsService>();

var app = builder.Build();

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
