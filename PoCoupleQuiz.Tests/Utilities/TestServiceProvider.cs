using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Radzen;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;
using Bunit;

namespace PoCoupleQuiz.Tests.Utilities;

public static class TestServiceExtensions
{
    public static void ConfigureTestServices(this IServiceCollection services)
    {
        // Add Radzen services
        services.AddScoped<DialogService>();
        services.AddScoped<NotificationService>();
        services.AddScoped<TooltipService>();
        services.AddScoped<ContextMenuService>();
    }
}

public class TestGameBuilder
{
    private readonly Mock<IQuestionService> _mockQuestionService = new();
    private readonly Mock<ITeamService> _mockTeamService = new();
    private readonly Mock<IGameHistoryService> _mockGameHistoryService = new();
    private readonly Mock<IGameStateService> _mockGameStateService = new();
    private readonly Game _game;

    public TestGameBuilder()
    {
        _game = new Game
        {
            Players = new List<Player>
            {
                new() { Name = "King", IsKingPlayer = true },
                new() { Name = "Player1" },
                new() { Name = "Player2" }
            },
            Questions = new List<GameQuestion>(),
            CurrentRound = 0
        };

        // Setup default mock behavior
        _mockQuestionService.Setup(x => x.GenerateQuestionAsync(It.IsAny<string>()))
            .ReturnsAsync(new Question { Text = "Test question", Category = QuestionCategory.Relationships });

        _mockGameStateService.Setup(x => x.CurrentGame).Returns(_game);
    }

    public TestGameBuilder WithPlayer(string name, bool isKing = false)
    {
        _game.AddPlayer(new Player { Name = name, IsKingPlayer = isKing });
        return this;
    }

    public TestGameBuilder WithRound(int round)
    {
        _game.CurrentRound = round;
        return this;
    }

    public (Game game, Dictionary<Type, Mock> mocks) Build()
    {
        return (_game, new Dictionary<Type, Mock>
        {
            { typeof(IQuestionService), _mockQuestionService },
            { typeof(ITeamService), _mockTeamService },
            { typeof(IGameHistoryService), _mockGameHistoryService },
            { typeof(IGameStateService), _mockGameStateService }
        });
    }
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(_mockQuestionService.Object);
        services.AddSingleton(_mockTeamService.Object);
        services.AddSingleton(_mockGameHistoryService.Object);
        services.AddSingleton(_mockGameStateService.Object);
        services.AddSingleton(Mock.Of<ILogger<object>>());
        services.AddSingleton(Mock.Of<NavigationManager>());
        services.AddSingleton(Mock.Of<NotificationService>());
    }
}
