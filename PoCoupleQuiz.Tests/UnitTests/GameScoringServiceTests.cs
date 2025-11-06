using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoCoupleQuiz.Tests.UnitTests;

[Trait("Category", "Unit")]
public class GameScoringServiceTests
{
    private readonly Mock<ILogger<GameScoringService>> _mockLogger;
    private readonly GameScoringService _scoringService;

    public GameScoringServiceTests()
    {
        _mockLogger = new Mock<ILogger<GameScoringService>>();
        _scoringService = new GameScoringService(_mockLogger.Object);
    }

    [Theory]
    [InlineData(true, 10)]
    [InlineData(false, 0)]
    public void CalculateRoundScore_ReturnsCorrectPoints(bool isMatch, int expectedScore)
    {
        // Act
        var score = _scoringService.CalculateRoundScore(isMatch);

        // Assert
        Assert.Equal(expectedScore, score);
    }

    [Fact]
    public async Task EvaluateAnswersAsync_NoKingAnswer_ReturnsEmptyResults()
    {
        // Arrange
        var game = CreateTestGame();
        var question = new GameQuestion { Question = "Test?" };
        var mockQuestionService = new Mock<IQuestionService>();

        // Act
        var results = await _scoringService.EvaluateAnswersAsync(game, question, mockQuestionService.Object);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task EvaluateAnswersAsync_WithAnswers_EvaluatesEachPlayer()
    {
        // Arrange
        var game = CreateTestGame();
        var question = new GameQuestion
        {
            Question = "Test?",
            KingPlayerAnswer = "Blue"
        };
        question.RecordPlayerAnswer("Player1", "Blue");
        question.RecordPlayerAnswer("Player2", "Red");

        var mockQuestionService = new Mock<IQuestionService>();
        mockQuestionService
            .Setup(x => x.CheckAnswerSimilarityAsync("Blue", "Blue"))
            .ReturnsAsync(true);
        mockQuestionService
            .Setup(x => x.CheckAnswerSimilarityAsync("Blue", "Red"))
            .ReturnsAsync(false);

        // Act
        var results = await _scoringService.EvaluateAnswersAsync(game, question, mockQuestionService.Object);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.True(results["Player1"]);
        Assert.False(results["Player2"]);
    }

    [Fact]
    public async Task EvaluateAnswersAsync_UpdatesPlayerScores()
    {
        // Arrange
        var game = CreateTestGame();
        var question = new GameQuestion
        {
            Question = "Test?",
            KingPlayerAnswer = "Blue"
        };
        question.RecordPlayerAnswer("Player1", "Blue");

        var mockQuestionService = new Mock<IQuestionService>();
        mockQuestionService
            .Setup(x => x.CheckAnswerSimilarityAsync("Blue", "Blue"))
            .ReturnsAsync(true);

        // Act
        await _scoringService.EvaluateAnswersAsync(game, question, mockQuestionService.Object);

        // Assert
        var player1 = game.Players.First(p => p.Name == "Player1");
        Assert.Equal(10, player1.Score); // KingPlayer mode = 10 points
    }

    private Game CreateTestGame()
    {
        return new Game
        {
            CurrentRound = 0,
            Players = new List<Player>
            {
                new Player { Name = "King", IsKingPlayer = true, Score = 0 },
                new Player { Name = "Player1", IsKingPlayer = false, Score = 0 },
                new Player { Name = "Player2", IsKingPlayer = false, Score = 0 }
            }
        };
    }
}
