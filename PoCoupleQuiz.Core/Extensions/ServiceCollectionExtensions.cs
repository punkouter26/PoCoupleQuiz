using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Core.Validators;

namespace PoCoupleQuiz.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPoCoupleQuizServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register validators
        services.AddSingleton<IValidator<string>, TeamNameValidator>();

        // Register Question Service - use Mock if Azure OpenAI is not configured
        var apiKey = configuration["AzureOpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "placeholder-key")
        {
            // Use mock service for local development without Azure OpenAI
            services.AddSingleton<IQuestionService, MockQuestionService>();
        }
        else
        {
            // Use real Azure OpenAI service
            services.AddSingleton<IQuestionService, AzureOpenAIQuestionService>();
        }

        // Use in-memory services when Azure Storage is not configured
        var storageConnectionString = configuration["AzureStorage:ConnectionString"];
        var useAzureStorage = !string.IsNullOrWhiteSpace(storageConnectionString) && 
                              !storageConnectionString.Contains("UseDevelopmentStorage");
        
        if (useAzureStorage)
        {
            services.AddSingleton<ITeamService, AzureTableTeamService>();
            services.AddScoped<IGameHistoryService, GameHistoryService>();
        }
        else
        {
            // Use in-memory services for local development without Azure Storage
            services.AddSingleton<ITeamService, InMemoryTeamService>();
            services.AddScoped<IGameHistoryService, InMemoryGameHistoryService>();
        }
        services.AddScoped<IGameStateService, GameStateService>();

        // Register Phase 2 refactored unified game engine (consolidates turn management and scoring)
        services.AddScoped<IGameEngine, GameEngine>();
        
        // Backward compatibility: map old interfaces to GameEngine for existing code
        services.AddScoped<IGameTurnManager>(sp => (IGameTurnManager)sp.GetRequiredService<IGameEngine>());
        services.AddScoped<IGameScoringService>(sp => (IGameScoringService)sp.GetRequiredService<IGameEngine>());

        return services;
    }
}
