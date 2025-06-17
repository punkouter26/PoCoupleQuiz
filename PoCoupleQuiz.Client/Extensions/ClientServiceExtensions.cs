using Microsoft.Extensions.DependencyInjection;
using PoCoupleQuiz.Core.Services;

namespace PoCoupleQuiz.Client.Extensions;

public static class ClientServiceExtensions
{
    public static IServiceCollection AddClientServices(this IServiceCollection services)
    {
        // Client uses HTTP services to communicate with the server
        services.AddScoped<IGameStateService, GameStateService>();
        services.AddScoped<ITeamService, HttpTeamService>();
        services.AddScoped<IQuestionService, MockQuestionService>(); // Client uses mock for simplicity
        services.AddScoped<IGameHistoryService, GameHistoryService>();
        
        return services;
    }
}
