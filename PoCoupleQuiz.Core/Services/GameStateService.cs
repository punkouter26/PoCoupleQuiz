using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

public interface IGameStateService
{
    Game? CurrentGame { get; }
    string CurrentPlayerName { get; }
    void InitializeGame(Game game, string currentPlayerName);
    void ClearGame();
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
}
