using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Client.Pages;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Bunit;
using Radzen;
using Microsoft.Extensions.DependencyInjection;
using GameModel = PoCoupleQuiz.Core.Models.Game;

namespace PoCoupleQuiz.Tests
{
    public class GameTests : BunitContext, IAsyncLifetime
    {
        private readonly Mock<IQuestionService> _mockQuestionService; private readonly Mock<ITeamService> _mockTeamService;
        private readonly Mock<IGameHistoryService> _mockGameHistoryService;
        private readonly Mock<ILogger<PoCoupleQuiz.Client.Pages.Index>> _mockLogger;
        private readonly Mock<IGameStateService> _mockGameStateService;
        private readonly Mock<NavigationManager> _mockNavigationManager;
        private readonly Mock<NotificationService> _mockNotificationService;
        private IServiceProvider _serviceProvider;

        public GameTests()
        {
            _mockQuestionService = new Mock<IQuestionService>();
            _mockTeamService = new Mock<ITeamService>();
            _mockGameHistoryService = new Mock<IGameHistoryService>();
            _mockLogger = new Mock<ILogger<PoCoupleQuiz.Client.Pages.Index>>();
            _mockGameStateService = new Mock<IGameStateService>();
            _mockNavigationManager = new Mock<NavigationManager>();
            _mockNotificationService = new Mock<NotificationService>();

            // Setup mock question
            _mockQuestionService.Setup(x => x.GenerateQuestionAsync(It.IsAny<string>()))
                .ReturnsAsync(new Question { Text = "Test question", Category = QuestionCategory.Relationships });

            // Setup mock game state
            var game = new GameModel
            {
                Players = new List<Player>
                {
                    new Player { Name = "King", IsKingPlayer = true },
                    new Player { Name = "Player1" },
                    new Player { Name = "Player2" }
                },
                Questions = new List<GameQuestion>(),
                CurrentRound = 0
            };
            _mockGameStateService.Setup(x => x.CurrentGame).Returns(game);

            // Register services
            Services.AddSingleton<IQuestionService>(_mockQuestionService.Object);
            Services.AddSingleton<ITeamService>(_mockTeamService.Object);
            Services.AddSingleton<IGameHistoryService>(_mockGameHistoryService.Object);
            Services.AddSingleton<ILogger<PoCoupleQuiz.Client.Pages.Index>>(_mockLogger.Object);
            Services.AddSingleton<IGameStateService>(_mockGameStateService.Object);
            Services.AddSingleton<NavigationManager>(_mockNavigationManager.Object);
            Services.AddSingleton<NotificationService>(_mockNotificationService.Object);
            // Add Radzen services
            Services.AddScoped<DialogService>();
            Services.AddScoped<TooltipService>();
            Services.AddScoped<ContextMenuService>();

            // Setup JSInterop for Radzen components
            JSInterop.SetupVoid("Radzen.preventArrows", _ => true);
            JSInterop.SetupVoid("Radzen.focusElement", _ => true);
            JSInterop.SetupVoid("Radzen.selectTab", _ => true);
            JSInterop.SetupVoid("Radzen.destroyTooltip", _ => true);
            JSInterop.SetupVoid("Radzen.showTooltip", _ => true);
            JSInterop.Setup<object>("Radzen.getProperty", _ => true).SetResult(new object());

            // Setup JSInterop for HelpTooltip component
            JSInterop.SetupVoid("eval", _ => true);

            // Store service provider for later cleanup
            _serviceProvider = Services.BuildServiceProvider();
        }

        public async Task InitializeAsync()
        {
            // No initialization needed
            await Task.CompletedTask;
        }
        public new async Task DisposeAsync()
        {
            // Dispose of services if needed
            if (_serviceProvider is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            // Call base dispose
            base.Dispose();
        }

        #region UI Rendering Tests
        [Trait("Category", "Unit")]
        [Fact]
        public async Task Index_ShouldRenderCorrectly()
        {
            // Arrange
            var cut = Render<PoCoupleQuiz.Client.Pages.Index>();

            // Act - Allow time for component to initialize
            await Task.Delay(100);

            // Assert - Check for game setup heading
            var heading = cut.Find("h6");
            Assert.Contains("Game Setup", heading.TextContent);
        }

        [Trait("Category", "Unit")]
        [Fact]
        public async Task Leaderboard_ShouldRenderCorrectly()
        {
            // Arrange
            var cut = Render<Leaderboard>();

            // Act - Allow time for component to initialize
            await Task.Delay(100);

            // Assert - Check for the Hall of Fame title (updated from h3 to h1 during UI/UX refresh)
            var heading = cut.Find("h1.leaderboard-title");
            Assert.Contains("Hall of Fame", heading.TextContent);
        }
        #endregion

        #region Game Logic Unit Tests
        // Note: Scoring logic tests are in GameScoringServiceTests.cs
        // The Game model no longer contains UpdateScores - scoring is handled by GameScoringService
        #endregion
    }
}