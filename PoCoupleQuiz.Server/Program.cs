using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Core.Extensions;
using PoCoupleQuiz.Server.Extensions;
using PoCoupleQuiz.Server.Middleware;
using PoCoupleQuiz.Server.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Linq;

namespace PoCoupleQuiz.Server
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            // Configure Serilog early for startup logging
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Configure basic Serilog for startup
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("=== PoCoupleQuiz Application Starting ===");
                Log.Information("Environment: {Environment}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production");
                Log.Information("Application Directory: {Directory}", Directory.GetCurrentDirectory());

                var builder = WebApplication.CreateBuilder(args);

                // Add Aspire ServiceDefaults for observability, resilience, and service discovery
                builder.AddServiceDefaults();

                // Add Aspire Azure Tables client (receives connection from AppHost/Azurite)
                builder.AddAzureTableServiceClient("tables");

                // Use Serilog configuration extension
                builder.Host.AddSerilogConfiguration();

                // Clear default logging providers since we're using Serilog
                builder.Logging.ClearProviders();
                // Add HttpClient for server-side use
                builder.Services.AddHttpClient();
                // Add services to the container.
                builder.Services.AddControllersWithViews();
                builder.Services.AddRazorPages();
                builder.Services.AddOpenApi();

                // Add health checks using extension method
                builder.Services.AddHealthCheckConfiguration(builder.Configuration);

                // Add HTTP context accessor for telemetry enrichment
                builder.Services.AddHttpContextAccessor();

                // Add Application Insights with custom telemetry initializer
                builder.Services.AddApplicationInsightsTelemetry();
                builder.Services.AddSingleton<Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer, PoCoupleQuiz.Server.Telemetry.CustomTelemetryInitializer>();

                // Configure OpenTelemetry using extension method
                builder.Services.AddOpenTelemetryConfiguration(
                    builder.Configuration,
                    builder.Environment.IsProduction());

                // Register application services using extension method
                builder.Services.AddPoCoupleQuizServices(builder.Configuration);

                Log.Information("Application services configured successfully");

                var app = builder.Build();

                Log.Information("Application built successfully, configuring middleware pipeline");

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    Log.Information("Development environment detected, enabling WebAssembly debugging and OpenAPI");
                    app.UseWebAssemblyDebugging();
                    app.MapOpenApi();
                }
                else
                {
                    Log.Information("Production environment detected, configuring production middleware");
                    app.UseExceptionHandler("/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                // Add global exception handling middleware
                app.UseMiddleware<GlobalExceptionMiddleware>();

                // Add custom telemetry middleware for performance tracking
                app.UseTelemetryMiddleware();

                // Configure Serilog request logging
                app.UseSerilogRequestLogging(options =>
                {
                    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                    options.GetLevel = (httpContext, elapsed, ex) =>
                    {
                        // Completely suppress logging for diagnostic network endpoints to reduce noise
                        if (httpContext.Request.Path.StartsWithSegments("/api/diagnostics/network"))
                            return LogEventLevel.Debug; // Will be filtered out since minimum level is Information

                        return LogEventLevel.Information;
                    };
                    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                    {
                        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "Unknown");
                        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown");
                        diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
                    };
                });

                // Skip HTTPS redirection for health check endpoints (Aspire uses HTTP for health checks)
                app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api/health") &&
                                        !context.Request.Path.StartsWithSegments("/health"),
                    appBuilder => appBuilder.UseHttpsRedirection());

                app.UseBlazorFrameworkFiles();
                app.UseStaticFiles();
                app.UseRouting();

                // Map health check endpoints
                app.MapHealthChecks("/api/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            status = report.Status.ToString(),
                            checks = report.Entries.Select(e => new
                            {
                                name = e.Key,
                                status = e.Value.Status.ToString(),
                                description = e.Value.Description,
                                duration = e.Value.Duration.TotalMilliseconds,
                                exception = e.Value.Exception?.Message
                            }),
                            totalDuration = report.TotalDuration.TotalMilliseconds
                        });
                        await context.Response.WriteAsync(result);
                    }
                });
                app.MapHealthChecks("/health");
                app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("live")
                });
                app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ready")
                });

                app.MapRazorPages();
                app.MapControllers();
                
                // Map Aspire default endpoints (health, alive)
                app.MapDefaultEndpoints();
                
                app.MapFallbackToFile("index.html");

                Log.Information("Application startup completed successfully, starting web host");

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "=== Application terminated unexpectedly ===");
                throw;
            }
            finally
            {
                Log.Information("=== Application Shutdown ===");
                Log.CloseAndFlush();
            }
        }
    }
}
