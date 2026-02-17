using Microsoft.Extensions.DependencyInjection;
using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Client.Services;

namespace PoCoupleQuiz.Client.Extensions;

public static class ClientServiceExtensions
{
    public static IServiceCollection AddClientServices(this IServiceCollection services)
    {
        // Client uses HTTP services to communicate with the server
        services.AddScoped<IGameStateService, GameStateService>();
        services.AddScoped<ITeamService, HttpTeamService>();
        services.AddScoped<IGameHistoryService, HttpGameHistoryService>();

        // Unified game engine for turn management and score calculation
        services.AddScoped<IGameEngine, PoCoupleQuiz.Core.Services.GameEngine>();
        services.AddScoped<IGameTurnManager>(sp => (IGameTurnManager)sp.GetRequiredService<IGameEngine>());
        services.AddScoped<IGameScoringService>(sp => (IGameScoringService)sp.GetRequiredService<IGameEngine>());

        // Question service needs to be available on client for game logic
        // Note: This will make HTTP calls to the server's API
        services.AddScoped<IQuestionService, HttpQuestionService>();

        // SignalR service for real-time game updates
        services.AddScoped<IGameHubService, GameHubService>();

        return services;
    }
}
