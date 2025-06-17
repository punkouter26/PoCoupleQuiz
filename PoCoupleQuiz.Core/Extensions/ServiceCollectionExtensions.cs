using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PoCoupleQuiz.Core.Services;

namespace PoCoupleQuiz.Core.Extensions;

public static class ServiceCollectionExtensions
{    public static IServiceCollection AddPoCoupleQuizServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register Question Service based on configuration
        if (ShouldUseMockQuestionService(configuration))
        {
            services.AddSingleton<IQuestionService, MockQuestionService>();
        }
        else
        {
            services.AddSingleton<IQuestionService, AzureOpenAIQuestionService>();
        }
        
        // Register Team Service based on configuration
        if (ShouldUseInMemoryTeamService(configuration))
        {
            services.AddSingleton<ITeamService, InMemoryTeamService>();
        }
        else
        {
            services.AddSingleton<ITeamService, AzureTableTeamService>();
        }
          // Register other services with consistent lifetimes
        if (ShouldUseInMemoryGameHistoryService(configuration))
        {
            services.AddScoped<IGameHistoryService, InMemoryGameHistoryService>();
        }
        else
        {
            services.AddScoped<IGameHistoryService, GameHistoryService>();
        }
        services.AddScoped<IGameStateService, GameStateService>();
        
        return services;
    }
      private static bool ShouldUseMockQuestionService(IConfiguration configuration)
    {
        var openAiEndpoint = configuration["AzureOpenAI:Endpoint"];
        var openAiKey = configuration["AzureOpenAI:Key"];
        
        return string.IsNullOrEmpty(openAiEndpoint) || 
               string.IsNullOrEmpty(openAiKey) ||
               openAiEndpoint.Contains("your-resource-name");
    }    private static bool ShouldUseInMemoryTeamService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"];
        
        return string.IsNullOrEmpty(connectionString) || 
               connectionString.Contains("UseDevelopmentStorage=false") ||
               connectionString.Contains("your-storage-account");
    }
    
    private static bool ShouldUseInMemoryGameHistoryService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"];
        
        return string.IsNullOrEmpty(connectionString) || 
               connectionString.Contains("UseDevelopmentStorage=false") ||
               connectionString.Contains("your-storage-account");
    }
}
