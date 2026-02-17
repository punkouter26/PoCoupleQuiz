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
using Azure.Identity;
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
                var currentEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                Log.Information("Environment: {Environment}", currentEnvironment);
                Log.Information("Application Directory: {Directory}", Directory.GetCurrentDirectory());

                var builder = WebApplication.CreateBuilder(args);

                // Add Azure Key Vault configuration (skip for Testing environment or if SKIP_KEYVAULT is set)
                var isTestEnvironment = string.Equals(currentEnvironment, "Testing", StringComparison.OrdinalIgnoreCase);
                var skipKeyVault = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SKIP_KEYVAULT"));
                var keyVaultUri = builder.Configuration["KeyVault:VaultUri"] 
                    ?? Environment.GetEnvironmentVariable("KEYVAULT_URI")
                    ?? "https://kv-poshared.vault.azure.net/";
                
                if (!string.IsNullOrEmpty(keyVaultUri) && !isTestEnvironment && !skipKeyVault)
                {
                    Log.Information("Loading configuration from Key Vault: {KeyVaultUri}", keyVaultUri);
                    builder.Configuration.AddAzureKeyVault(
                        new Uri(keyVaultUri),
                        new DefaultAzureCredential());
                }
                else if (isTestEnvironment || skipKeyVault)
                {
                    Log.Information("Skipping Key Vault configuration (Testing={IsTest}, SkipKeyVault={SkipKV})", 
                        isTestEnvironment, skipKeyVault);
                }

                // Add Aspire ServiceDefaults for observability, resilience, and service discovery
                // Commented out: ServiceDefaults project not present
                // builder.AddServiceDefaults();

                // Add Azure Tables client only if Azure Storage is configured
                // When using Azure Storage from Key Vault (USE_AZURE_STORAGE=true), the connection string
                // comes from Key Vault secret: PoCoupleQuiz--AzureStorage--ConnectionString
                // Otherwise, Aspire provides the connection from AppHost (Azurite or provisioned storage)
                var useAzureStorage = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("USE_AZURE_STORAGE"));
                var storageConnectionString = builder.Configuration["PoCoupleQuiz:AzureStorage:ConnectionString"];
                
                if (useAzureStorage && !string.IsNullOrEmpty(storageConnectionString))
                {
                    Log.Information("Using Azure Storage connection from Key Vault");
                    builder.Services.AddSingleton(new Azure.Data.Tables.TableServiceClient(storageConnectionString));
                }
                else if (!string.IsNullOrEmpty(storageConnectionString))
                {
                    // Use Aspire-managed Azure Tables client (from AppHost/Azurite)
                    builder.AddAzureTableServiceClient("tables");
                }
                else
                {
                    Log.Information("Azure Storage not configured - using in-memory services");
                }

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
                
                // Add Swagger/OpenAPI with UI
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
                    {
                        Title = "PoCoupleQuiz API",
                        Version = "v1",
                        Description = "Interactive quiz game API for couples and friends"
                    });
                });

                // Add SignalR for real-time features
                builder.Services.AddSignalR();

                // Add session services for user tracking in logs
                builder.Services.AddDistributedMemoryCache();
                builder.Services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromHours(2);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                    options.Cookie.Name = ".PoCoupleQuiz.Session";
                });

                // Add health checks using extension method
                builder.Services.AddHealthCheckConfiguration(builder.Configuration);

                // Add HTTP context accessor for telemetry enrichment
                builder.Services.AddHttpContextAccessor();

                // Add Application Insights
                builder.Services.AddApplicationInsightsTelemetry();

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
                    
                    // Enable Swagger UI in development
                    app.UseSwagger();
                    app.UseSwaggerUI(options =>
                    {
                        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PoCoupleQuiz API v1");
                        options.RoutePrefix = "swagger";
                        options.DocumentTitle = "PoCoupleQuiz API Explorer";
                    });
                }
                else
                {
                    Log.Information("Production environment detected, configuring production middleware");;
                    app.UseExceptionHandler("/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                // Add global exception handling middleware
                app.UseMiddleware<GlobalExceptionMiddleware>();

                // Add session support for user tracking
                app.UseSession();

                // Enrich logs with user and session context
                app.UseLogContextEnrichment();

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

                // Map health check endpoints (custom detailed endpoint)
                // Note: /health and /alive are mapped by MapDefaultEndpoints from ServiceDefaults
                app.MapHealthChecks("/api/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        context.Response.ContentType = "application/json";
                        var assembly = typeof(Program).Assembly;
                        var version = assembly.GetName().Version?.ToString() ?? "1.0.0";
                        var buildDate = System.IO.File.GetLastWriteTimeUtc(assembly.Location).ToString("o");
                        
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            status = report.Status.ToString(),
                            version = version,
                            buildDate = buildDate,
                            environment = app.Environment.EnvironmentName,
                            machineName = Environment.MachineName,
                            timestamp = DateTime.UtcNow.ToString("o"),
                            checks = report.Entries.Select(e => new
                            {
                                name = e.Key,
                                status = e.Value.Status.ToString(),
                                description = e.Value.Description,
                                duration = e.Value.Duration.TotalMilliseconds,
                                tags = e.Value.Tags.ToArray(),
                                exception = e.Value.Exception?.Message
                            }),
                            totalDuration = report.TotalDuration.TotalMilliseconds,
                            aiMetadata = new
                            {
                                serviceName = "PoCoupleQuiz",
                                aspireVersion = "13.1.0",
                                runtimeVersion = Environment.Version.ToString()
                            }
                        });
                        await context.Response.WriteAsync(result);
                    }
                });
                app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ready")
                });

                app.MapRazorPages();
                app.MapControllers();
                
                // Map SignalR hub for real-time game updates
                app.MapHub<Hubs.GameHub>("/hubs/game");
                
                // Map Aspire default endpoints (health, alive)
                // Commented out: ServiceDefaults project not present
                // app.MapDefaultEndpoints();
                
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
