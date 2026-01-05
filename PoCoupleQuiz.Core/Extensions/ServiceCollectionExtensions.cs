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

        services.AddSingleton<ITeamService, AzureTableTeamService>();
        services.AddScoped<IGameHistoryService, GameHistoryService>();
        services.AddScoped<IGameStateService, GameStateService>();

        // Register Phase 2 refactored services
        services.AddScoped<IGameTurnManager, GameTurnManager>();
        services.AddScoped<IGameScoringService, GameScoringService>();
        services.AddSingleton<IPromptBuilder, PromptBuilder>();
        services.AddSingleton<IQuestionCache, QuestionCache>();

        return services;
    }
}
