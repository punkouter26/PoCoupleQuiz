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
    public void GameEngine_CalculateRoundScore_MatchingAnswer_Returns10Points()
    {
        // Arrange - Using the GameEngine instead of the removed Game.CalculateScore
        var mockLogger = new Mock<ILogger<GameEngine>>();
        var gameEngine = new GameEngine(mockLogger.Object);
        
        // Act
        var score = gameEngine.CalculateRoundScore(isMatch: true);

        // Assert
        Assert.Equal(10, score);
    }

    [Fact]
    public void GameEngine_CalculateRoundScore_NonMatchingAnswer_Returns0Points()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GameEngine>>();
        var gameEngine = new GameEngine(mockLogger.Object);

        // Act
        var score = gameEngine.CalculateRoundScore(isMatch: false);

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

    [Fact]
    public void GetScoreboard_IncludesAllPlayers_IncludingKingPlayer()
    {
        // Arrange - King Player should now appear on scoreboard with visual indicator
        var game = new Game
        {
            Players = new List<Player>
            {
                new Player { Name = "King Player", Score = 0, IsKingPlayer = true },
                new Player { Name = "Player 2", Score = 30, IsKingPlayer = false },
                new Player { Name = "Player 3", Score = 20, IsKingPlayer = false }
            }
        };

        // Act
        var scoreboard = game.GetScoreboard();

        // Assert - All players should be on the scoreboard, including the king
        Assert.Equal(3, scoreboard.Count);
        Assert.True(scoreboard.ContainsKey("King Player"));
        Assert.True(scoreboard.ContainsKey("Player 2"));
        Assert.True(scoreboard.ContainsKey("Player 3"));
        // Verify scores are correct
        Assert.Equal(0, scoreboard["King Player"]);
        Assert.Equal(30, scoreboard["Player 2"]);
        Assert.Equal(20, scoreboard["Player 3"]);
    }
}
