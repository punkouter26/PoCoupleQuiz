using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
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
        services.AddScoped<IQuestionService, HttpQuestionService>();

        // SignalR service — scoped (behaves like singleton in WASM root scope).
        // IAccessTokenProvider is null in dev mode (MSAL not registered); cookie auth handles SignalR instead.
        services.AddScoped<IGameHubService>(sp =>
        {
            var config      = sp.GetRequiredService<IConfiguration>();
            var nav         = sp.GetRequiredService<NavigationManager>();
            var logger      = sp.GetRequiredService<ILogger<GameHubService>>();
            var tokenProvider = sp.GetService<IAccessTokenProvider>(); // null in dev
            return new GameHubService(config, nav, logger, tokenProvider);
        });

        return services;
    }
}
