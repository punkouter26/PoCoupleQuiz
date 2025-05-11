using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Web.Pages;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Bunit;
using MudBlazor;
using MudBlazor.Services;
using Microsoft.Extensions.DependencyInjection;
using GamePage = PoCoupleQuiz.Web.Pages.Game;
using GameModel = PoCoupleQuiz.Core.Models.Game;

namespace PoCoupleQuiz.Tests
{
    public class GameTests : TestContext, IAsyncLifetime
    {
        private readonly Mock<IQuestionService> _mockQuestionService;
        private readonly Mock<ITeamService> _mockTeamService;
        private readonly Mock<IGameHistoryService> _mockGameHistoryService;
        private readonly Mock<ILogger<GamePage>> _mockLogger;
        private readonly Mock<IGameStateService> _mockGameStateService;
        private readonly Mock<NavigationManager> _mockNavigationManager;
        private readonly Mock<ISnackbar> _mockSnackbar;
        private IAsyncDisposable _disposable;

        public GameTests()
        {
            _mockQuestionService = new Mock<IQuestionService>();
            _mockTeamService = new Mock<ITeamService>();
            _mockGameHistoryService = new Mock<IGameHistoryService>();
            _mockLogger = new Mock<ILogger<GamePage>>();
            _mockGameStateService = new Mock<IGameStateService>();
            _mockNavigationManager = new Mock<NavigationManager>();
            _mockSnackbar = new Mock<ISnackbar>();

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
            Services.AddSingleton<ILogger<GamePage>>(_mockLogger.Object);
            Services.AddSingleton<IGameStateService>(_mockGameStateService.Object);
            Services.AddSingleton<NavigationManager>(_mockNavigationManager.Object);
            Services.AddSingleton<ISnackbar>(_mockSnackbar.Object);
            
            // Add MudBlazor services with proper disposal
            Services.AddMudServices(options => 
            {
                options.SnackbarConfiguration.ShowTransitionDuration = 10;
                options.SnackbarConfiguration.HideTransitionDuration = 10;
            });
            
            // Setup JSInterop for MudBlazor components
            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            JSInterop.SetupVoid("mudKeyInterceptor.disconnect", _ => true);
            
            // Setup MudBlazor services and store disposable
            var serviceProvider = Services.AddMudServices(options => 
            {
                options.SnackbarConfiguration.ShowTransitionDuration = 10;
                options.SnackbarConfiguration.HideTransitionDuration = 10;
            })
            .BuildServiceProvider();
            
            _disposable = serviceProvider.GetRequiredService<MudBlazor.Services.KeyInterceptorService>();
        }

        public async Task InitializeAsync()
        {
            // No initialization needed
            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            if (_disposable != null)
            {
                await _disposable.DisposeAsync();
            }
        }

        [Fact]
        public async Task Game_ShouldNotShowAnswersDuringRounds()
        {
            // Arrange
            var cut = Render<GamePage>();

            // Act - use public methods instead of protected OnInitializedAsync
            // Initialize the component through rendering
            await Task.Delay(100); // Allow time for component to initialize
            await cut.InvokeAsync(() => cut.Instance.SubmitKingAnswer());
            await cut.InvokeAsync(() => cut.Instance.SubmitPlayerGuess("Player1"));
            await cut.InvokeAsync(() => cut.Instance.SubmitPlayerGuess("Player2"));

            // Assert
            var currentQuestion = cut.Instance.GetCurrentQuestion();
            Assert.Empty(currentQuestion.PlayersMatched);
        }

        [Fact]
        public async Task Game_ShouldShowAllAnswersAtEnd()
        {
            // Arrange
            var cut = Render<GamePage>();

            // Act - use public methods instead of protected OnInitializedAsync
            // Initialize the component through rendering
            await Task.Delay(100); // Allow time for component to initialize
            await cut.InvokeAsync(() => cut.Instance.SubmitKingAnswer());
            await cut.InvokeAsync(() => cut.Instance.SubmitPlayerGuess("Player1"));
            await cut.InvokeAsync(() => cut.Instance.SubmitPlayerGuess("Player2"));
            await cut.InvokeAsync(() => cut.Instance.EndGame());

            // Assert
            var currentQuestion = cut.Instance.GetCurrentQuestion();
            Assert.NotEmpty(currentQuestion.PlayersMatched);
        }

        [Fact]
        public async Task Game_ShouldMaintainAnswerHistory()
        {
            // Arrange
            var cut = Render<GamePage>();

            // Act - use public methods instead of protected OnInitializedAsync
            // Initialize the component through rendering
            await Task.Delay(100); // Allow time for component to initialize
            await cut.InvokeAsync(() => cut.Instance.SubmitKingAnswer());
            await cut.InvokeAsync(() => cut.Instance.SubmitPlayerGuess("Player1"));
            await cut.InvokeAsync(() => cut.Instance.SubmitPlayerGuess("Player2"));
            await cut.InvokeAsync(() => cut.Instance.EndGame());

            // Assert
            var currentQuestion = cut.Instance.GetCurrentQuestion();
            Assert.NotNull(currentQuestion.KingPlayerAnswer);
            Assert.NotEmpty(currentQuestion.PlayerAnswers);
        }

        [Fact]
        public async Task Game_ShouldNotShowMatchingStatusDuringGame()
        {
            // Arrange
            var cut = Render<GamePage>();

            // Act - use public methods instead of protected OnInitializedAsync
            // Initialize the component through rendering
            await Task.Delay(100); // Allow time for component to initialize
            await cut.InvokeAsync(() => cut.Instance.SubmitKingAnswer());
            await cut.InvokeAsync(() => cut.Instance.SubmitPlayerGuess("Player1"));
            await cut.InvokeAsync(() => cut.Instance.SubmitPlayerGuess("Player2"));

            // Assert
            var currentQuestion = cut.Instance.GetCurrentQuestion();
            Assert.False(currentQuestion.HasPlayerMatched("Player1"));
            Assert.False(currentQuestion.HasPlayerMatched("Player2"));
        }

        [Fact]
        public async Task Game_ShouldShowMatchingStatusAtEnd()
        {
            // Arrange
            var cut = Render<GamePage>();

            // Act - use public methods instead of protected OnInitializedAsync
            // Initialize the component through rendering
            await Task.Delay(100); // Allow time for component to initialize
            await cut.InvokeAsync(() => cut.Instance.SubmitKingAnswer());
            await cut.InvokeAsync(() => cut.Instance.SubmitPlayerGuess("Player1"));
            await cut.InvokeAsync(() => cut.Instance.SubmitPlayerGuess("Player2"));
            await cut.InvokeAsync(() => cut.Instance.EndGame());

            // Assert
            var currentQuestion = cut.Instance.GetCurrentQuestion();
            Assert.True(currentQuestion.HasPlayerMatched("Player1"));
            Assert.True(currentQuestion.HasPlayerMatched("Player2"));
        }
    }
}
