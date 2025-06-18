using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

public interface IGameStateService
{
    Game? CurrentGame { get; }
    string CurrentPlayerName { get; }
    void InitializeGame(Game game, string currentPlayerName);
    void ClearGame();
    void SetCurrentPlayer(string playerName);
    string GetNextGuessingPlayer();
    List<string> GetGuessingPlayers();
    bool AllGuessingPlayersAnswered(int roundIndex);
}

public class GameStateService : IGameStateService
{
    public Game? CurrentGame { get; private set; }
    public string CurrentPlayerName { get; private set; } = "";

    public void InitializeGame(Game game, string currentPlayerName)
    {
        CurrentGame = game;
        CurrentPlayerName = currentPlayerName;
    }

    public void ClearGame()
    {
        CurrentGame = null;
        CurrentPlayerName = "";
    }

    public void SetCurrentPlayer(string playerName)
    {
        CurrentPlayerName = playerName;
    }

    public string GetNextGuessingPlayer()
    {
        if (CurrentGame == null) return "";
        
        var guessingPlayers = CurrentGame.Players.Where(p => !p.IsKingPlayer).ToList();
        if (!guessingPlayers.Any()) return "";
        
        var currentIndex = guessingPlayers.FindIndex(p => p.Name == CurrentPlayerName);
        var nextIndex = (currentIndex + 1) % guessingPlayers.Count;
        
        return guessingPlayers[nextIndex].Name;
    }

    public List<string> GetGuessingPlayers()
    {
        if (CurrentGame == null) return new List<string>();
        
        return CurrentGame.Players.Where(p => !p.IsKingPlayer).Select(p => p.Name).ToList();
    }

    public bool AllGuessingPlayersAnswered(int roundIndex)
    {
        if (CurrentGame == null || roundIndex >= CurrentGame.Questions.Count) return false;
        
        var question = CurrentGame.Questions[roundIndex];
        var guessingPlayers = CurrentGame.Players.Where(p => !p.IsKingPlayer);
        
        return guessingPlayers.All(p => question.HasPlayerAnswered(p.Name));
    }
}
