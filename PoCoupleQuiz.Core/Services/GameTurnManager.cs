using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

/// <summary>
/// Service for managing game turn progression and player state.
/// </summary>
public interface IGameTurnManager
{
    /// <summary>
    /// Initializes turn state for a new round.
    /// </summary>
    void InitializeTurn(Game game, GameQuestion question);

    /// <summary>
    /// Advances to the next player's turn.
    /// </summary>
    /// <returns>True if there are more players, false if round is complete.</returns>
    bool AdvanceToNextPlayer(Game game, GameQuestion question);

    /// <summary>
    /// Gets the name of the current player.
    /// </summary>
    string GetCurrentPlayerName(Game game, GameQuestion question);

    /// <summary>
    /// Determines if it's currently the king player's turn.
    /// </summary>
    bool IsKingPlayerTurn(GameQuestion question);

    /// <summary>
    /// Gets the current guessing player index.
    /// </summary>
    int GetCurrentGuessingPlayerIndex(Game game, GameQuestion question);
}

public class GameTurnManager : IGameTurnManager
{
    private int _currentGuessingPlayerIndex;

    public void InitializeTurn(Game game, GameQuestion question)
    {
        _currentGuessingPlayerIndex = 0;
    }

    public bool AdvanceToNextPlayer(Game game, GameQuestion question)
    {
        if (IsKingPlayerTurn(question))
        {
            // King has answered, move to first guessing player
            return true;
        }

        // Advance guessing player index
        _currentGuessingPlayerIndex++;
        var guessingPlayers = game.Players.Where(p => !p.IsKingPlayer).ToList();
        
        return _currentGuessingPlayerIndex < guessingPlayers.Count;
    }

    public string GetCurrentPlayerName(Game game, GameQuestion question)
    {
        if (IsKingPlayerTurn(question))
        {
            return game.KingPlayer?.Name ?? "";
        }

        var guessingPlayers = game.Players.Where(p => !p.IsKingPlayer).ToList();
        
        // Find unanswered players
        var unansweredPlayers = guessingPlayers
            .Where(p => !question.HasPlayerAnswered(p.Name))
            .ToList();

        if (unansweredPlayers.Any())
        {
            _currentGuessingPlayerIndex = guessingPlayers.IndexOf(unansweredPlayers.First());
            return unansweredPlayers.First().Name;
        }

        return guessingPlayers.ElementAtOrDefault(_currentGuessingPlayerIndex)?.Name ?? "";
    }

    public bool IsKingPlayerTurn(GameQuestion question)
    {
        return string.IsNullOrEmpty(question.KingPlayerAnswer);
    }

    public int GetCurrentGuessingPlayerIndex(Game game, GameQuestion question)
    {
        return _currentGuessingPlayerIndex;
    }
}
