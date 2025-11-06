using Xunit;
using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace PoCoupleQuiz.Tests.UnitTests;

[Trait("Category", "Unit")]
public class GameTurnManagerTests
{
    private readonly GameTurnManager _turnManager;

    public GameTurnManagerTests()
    {
        _turnManager = new GameTurnManager();
    }

    [Fact]
    public void InitializeTurn_ResetsState()
    {
        // Arrange
        var game = CreateTestGame();
        var question = new GameQuestion { Question = "Test?" };

        // Act
        _turnManager.InitializeTurn(game, question);

        // Assert
        Assert.Equal(0, _turnManager.GetCurrentGuessingPlayerIndex(game, question));
    }

    [Fact]
    public void IsKingPlayerTurn_NoKingAnswer_ReturnsTrue()
    {
        // Arrange
        var question = new GameQuestion
        {
            Question = "Test?",
            KingPlayerAnswer = ""
        };

        // Act
        var result = _turnManager.IsKingPlayerTurn(question);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsKingPlayerTurn_HasKingAnswer_ReturnsFalse()
    {
        // Arrange
        var question = new GameQuestion
        {
            Question = "Test?",
            KingPlayerAnswer = "Answer"
        };

        // Act
        var result = _turnManager.IsKingPlayerTurn(question);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetCurrentPlayerName_KingTurn_ReturnsKingName()
    {
        // Arrange
        var game = CreateTestGame();
        var question = new GameQuestion { Question = "Test?" };
        _turnManager.InitializeTurn(game, question);

        // Act
        var playerName = _turnManager.GetCurrentPlayerName(game, question);

        // Assert
        Assert.Equal("King", playerName);
    }

    [Fact]
    public void GetCurrentPlayerName_GuessingTurn_ReturnsGuessingPlayer()
    {
        // Arrange
        var game = CreateTestGame();
        var question = new GameQuestion
        {
            Question = "Test?",
            KingPlayerAnswer = "King's answer"
        };
        _turnManager.InitializeTurn(game, question);

        // Act
        var playerName = _turnManager.GetCurrentPlayerName(game, question);

        // Assert
        Assert.Contains(playerName, new[] { "Player1", "Player2" });
    }

    [Fact]
    public void AdvanceToNextPlayer_FromKingToGuessing_ReturnsTrue()
    {
        // Arrange
        var game = CreateTestGame();
        var question = new GameQuestion { Question = "Test?" };
        _turnManager.InitializeTurn(game, question);

        // Act
        var hasMore = _turnManager.AdvanceToNextPlayer(game, question);

        // Assert
        Assert.True(hasMore);
    }

    [Fact]
    public void AdvanceToNextPlayer_AllPlayersAnswered_ReturnsFalse()
    {
        // Arrange
        var game = CreateTestGame();
        var question = new GameQuestion
        {
            Question = "Test?",
            KingPlayerAnswer = "King's answer"
        };
        question.RecordPlayerAnswer("Player1", "Answer1");
        question.RecordPlayerAnswer("Player2", "Answer2");
        _turnManager.InitializeTurn(game, question);

        // Advance through all players
        _turnManager.AdvanceToNextPlayer(game, question);
        var hasMore = _turnManager.AdvanceToNextPlayer(game, question);

        // Assert
        Assert.False(hasMore);
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
