using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Core.Extensions;
using PoCoupleQuiz.Server.Middleware;
using PoCoupleQuiz.Server.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
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
            // Configure Serilog early, before the host is built
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Ensure DEBUG directory exists
            var debugPath = Path.Combine(Directory.GetCurrentDirectory(), "DEBUG");
            if (!Directory.Exists(debugPath))
                Directory.CreateDirectory(debugPath);

            // Configure Serilog with shared log file for testing compatibility
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("=== PoCoupleQuiz Application Starting ===");
                Log.Information("Environment: {Environment}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production");
                Log.Information("Application Directory: {Directory}", Directory.GetCurrentDirectory());

                var builder = WebApplication.CreateBuilder(args);

                // Use Serilog for logging
                builder.Host.UseSerilog();

                // Clear default logging providers since we're using Serilog
                builder.Logging.ClearProviders();
                // Add HttpClient for server-side use
                builder.Services.AddHttpClient();
                // Add services to the container.
                builder.Services.AddControllersWithViews();
                builder.Services.AddRazorPages();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();            // Add health checks
                builder.Services.AddHealthChecks()
                    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"), tags: new[] { "live" })
                    .AddCheck<AzureTableStorageHealthCheck>("azure_table_storage", tags: new[] { "ready" })
                    .AddCheck<AzureOpenAIHealthCheck>("azure_openai", tags: new[] { "ready" });

                // Add Application Insights
                builder.Services.AddApplicationInsightsTelemetry();

                // Register application services using extension method
                builder.Services.AddPoCoupleQuizServices(builder.Configuration);

                Log.Information("Application services configured successfully");

                var app = builder.Build();

                Log.Information("Application built successfully, configuring middleware pipeline");

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    Log.Information("Development environment detected, enabling WebAssembly debugging and Swagger");
                    app.UseWebAssemblyDebugging();
                    app.UseSwagger();
                    app.UseSwaggerUI();
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

                app.UseHttpsRedirection();

                app.UseBlazorFrameworkFiles();
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
