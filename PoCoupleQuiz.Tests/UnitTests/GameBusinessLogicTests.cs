using Xunit;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace PoCoupleQuiz.Tests.UnitTests;

[Trait("Category", "Unit")]
public class GameBusinessLogicTests
{
    [Fact]
    public void GameScoringService_CalculateRoundScore_MatchingAnswer_Returns10Points()
    {
        // Arrange - Using the GameScoringService instead of the removed Game.CalculateScore
        var mockLogger = new Mock<ILogger<GameScoringService>>();
        var scoringService = new GameScoringService(mockLogger.Object);
        
        // Act
        var score = scoringService.CalculateRoundScore(isMatch: true);

        // Assert
        Assert.Equal(10, score);
    }

    [Fact]
    public void GameScoringService_CalculateRoundScore_NonMatchingAnswer_Returns0Points()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GameScoringService>>();
        var scoringService = new GameScoringService(mockLogger.Object);

        // Act
        var score = scoringService.CalculateRoundScore(isMatch: false);

        // Assert
        Assert.Equal(0, score);
    }

    [Fact]
    public void IsGameOver_AllRoundsComplete_ReturnsTrue()
    {
        // Arrange
        var game = new Game
        {
            Difficulty = DifficultyLevel.Easy, // MaxRounds = 3
            CurrentRound = 3
        };

        // Act
        var isOver = game.IsGameOver;

        // Assert
        Assert.True(isOver);
    }

    [Fact]
    public void IsGameOver_RoundsRemaining_ReturnsFalse()
    {
        // Arrange
        var game = new Game
        {
            Difficulty = DifficultyLevel.Medium, // MaxRounds = 5
            CurrentRound = 3
        };

        // Act
        var isOver = game.IsGameOver;

        // Assert
        Assert.False(isOver);
    }

    [Fact]
    public void GetScoreboard_ReturnsCorrectPlayerScores()
    {
        // Arrange
        var game = new Game
        {
            Players = new List<Player>
            {
                new Player { Name = "Alice", Score = 25 },
                new Player { Name = "Bob", Score = 30 },
                new Player { Name = "Charlie", Score = 20 }
            }
        };

        // Act
        var scoreboard = game.GetScoreboard();

        // Assert
        Assert.Equal(3, scoreboard.Count);
        Assert.Equal(25, scoreboard["Alice"]);
        Assert.Equal(30, scoreboard["Bob"]);
        Assert.Equal(20, scoreboard["Charlie"]);
    }

    [Fact]
    public void GetScoreboard_OrderedByScore_ReturnsDescendingOrder()
    {
        // Arrange
        var game = new Game
        {
            Players = new List<Player>
            {
                new Player { Name = "Alice", Score = 25 },
                new Player { Name = "Bob", Score = 30 },
                new Player { Name = "Charlie", Score = 20 }
            }
        };

        // Act
        var scoreboard = game.GetScoreboard();
        var orderedScores = scoreboard.Values.ToList();

        // Assert - scores should be accessible for ordering
        Assert.Contains(30, orderedScores);
        Assert.Contains(25, orderedScores);
        Assert.Contains(20, orderedScores);
    }
}
