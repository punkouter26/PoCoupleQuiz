using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Core.Extensions;
using PoCoupleQuiz.Server.Middleware;
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

            // Clear existing log file on startup (overwrite behavior)
            var logPath = Path.Combine(debugPath, "log.txt");
            if (File.Exists(logPath))
                File.Delete(logPath);

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
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"));

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
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
                    diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
                };
            });

            app.UseHttpsRedirection();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles(); app.UseRouting();

            // Map health check endpoints
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
