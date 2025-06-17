using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PoCoupleQuiz.Core.Services;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace PoCoupleQuiz.Tests.Utilities;

public class CustomWebApplicationFactory : WebApplicationFactory<PoCoupleQuiz.Server.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var projectDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "PoCoupleQuiz.Server"));
        var clientAppContentRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "PoCoupleQuiz.Client", "wwwroot"));

        builder.UseContentRoot(projectDir);
        builder.UseWebRoot(clientAppContentRoot);        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Use in-memory configuration for testing
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureStorage:ConnectionString"] = "UseDevelopmentStorage=true", // Use real Azurite for integration tests
                ["AzureOpenAI:Endpoint"] = "https://test.openai.azure.com/",
                ["AzureOpenAI:Key"] = "test-key-for-testing-only",
                ["AzureOpenAI:DeploymentName"] = "test-deployment",
                ["ApplicationInsights:ConnectionString"] = "", // Disable for tests
                ["Logging:LogLevel:Default"] = "Warning" // Reduce log noise in tests
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing services and replace with test implementations
            var descriptorsToRemove = services.Where(d => 
                d.ServiceType == typeof(IQuestionService) ||
                d.ServiceType == typeof(ITeamService) ||
                d.ServiceType == typeof(IGameHistoryService) ||
                d.ServiceType == typeof(IGameStateService)).ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Add test implementations
            services.AddSingleton<IQuestionService, MockQuestionService>();
            services.AddSingleton<ITeamService, InMemoryTeamService>();
            services.AddSingleton<IGameHistoryService, InMemoryGameHistoryService>();
            services.AddScoped<IGameStateService, GameStateService>();

            // Add logging for tests
            services.AddLogging(builder => builder.AddConsole());
        });

        builder.UseEnvironment("Testing");
    }

}
