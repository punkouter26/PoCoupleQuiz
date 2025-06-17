using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PoCoupleQuiz.Core.Services;
using Microsoft.Extensions.Logging;

namespace PoCoupleQuiz.Tests.Utilities;

public class CustomWebApplicationFactory : WebApplicationFactory<PoCoupleQuiz.Server.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Use in-memory configuration for testing
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureStorage:ConnectionString"] = "UseDevelopmentStorage=false",
                ["AzureOpenAI:Endpoint"] = "https://test.openai.azure.com/",
                ["AzureOpenAI:Key"] = "test-key",
                ["AzureOpenAI:DeploymentName"] = "test-deployment"
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
