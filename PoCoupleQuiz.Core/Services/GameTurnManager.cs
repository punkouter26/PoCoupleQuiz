using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

/// <summary>
/// Service for managing game turn progression and player state.
/// Stateless - uses GameQuestion to track which players have answered.
/// </summary>
public interface IGameTurnManager
{
    /// <summary>
    /// Gets the name of the current player who should answer.
    /// </summary>
    string GetCurrentPlayerName(Game game, GameQuestion question);

    /// <summary>
    /// Determines if it's currently the king player's turn.
    /// </summary>
    bool IsKingPlayerTurn(GameQuestion question);

    /// <summary>
    /// Gets the index of the current guessing player.
    /// </summary>
    int GetCurrentGuessingPlayerIndex(Game game, GameQuestion question);

    /// <summary>
    /// Checks if there are more guessing players who need to answer.
    /// </summary>
    bool HasMoreGuessingPlayers(Game game, GameQuestion question);
}

public class GameTurnManager : IGameTurnManager
{
    public string GetCurrentPlayerName(Game game, GameQuestion question)
    {
        if (IsKingPlayerTurn(question))
        {
            return game.KingPlayer?.Name ?? "";
        }

        var guessingPlayers = game.Players.Where(p => !p.IsKingPlayer).ToList();
        var unansweredPlayers = guessingPlayers
            .Where(p => !question.HasPlayerAnswered(p.Name))
            .ToList();

        return unansweredPlayers.FirstOrDefault()?.Name ?? "";
    }

    public bool IsKingPlayerTurn(GameQuestion question)
    {
        return string.IsNullOrEmpty(question.KingPlayerAnswer);
    }

    public int GetCurrentGuessingPlayerIndex(Game game, GameQuestion question)
    {
        var guessingPlayers = game.Players.Where(p => !p.IsKingPlayer).ToList();
        var currentPlayerName = GetCurrentPlayerName(game, question);
        return guessingPlayers.FindIndex(p => p.Name == currentPlayerName);
    }

    public bool HasMoreGuessingPlayers(Game game, GameQuestion question)
    {
        var guessingPlayers = game.Players.Where(p => !p.IsKingPlayer);
        return guessingPlayers.Any(p => !question.HasPlayerAnswered(p.Name));
    }
}
