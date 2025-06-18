using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace PoCoupleQuiz.Tests.Utilities
{
    public class TestConfiguration
    {
        public static IConfiguration GetConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"AzureOpenAI:Endpoint", "https://your-test-openai-resource.openai.azure.com/"}, // Replace with a valid test endpoint
                {"AzureOpenAI:Key", "your-test-openai-key"}, // Replace with a valid test key
                {"AzureOpenAI:DeploymentName", "your-test-deployment-name"}, // Replace with your test deployment name
                // Add other configuration settings as needed for tests
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            return configuration;
        }
    }
}
