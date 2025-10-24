using PoCoupleQuiz.Core.Models;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<GameStateService> _logger;

    public Game? CurrentGame { get; private set; }
    public string CurrentPlayerName { get; private set; } = "";

    public GameStateService(ILogger<GameStateService> logger)
    {
        _logger = logger;
        _logger.LogDebug("GameStateService initialized");
    }

    public void InitializeGame(Game game, string currentPlayerName)
    {
        CurrentGame = game;
        CurrentPlayerName = currentPlayerName;

        _logger.LogInformation("Game initialized - Current Player: {PlayerName}, Total Players: {PlayerCount}, Max Rounds: {MaxRounds}",
            currentPlayerName, game.Players.Count, game.MaxRounds);

        _logger.LogDebug("Game players: {Players}", string.Join(", ", game.Players.Select(p => $"{p.Name}({(p.IsKingPlayer ? "King" : "Guesser")})")));
    }

    public void ClearGame()
    {
        var hadGame = CurrentGame != null;
        CurrentGame = null;
        CurrentPlayerName = "";

        _logger.LogInformation("Game cleared - Had active game: {HadGame}", hadGame);
    }

    public void SetCurrentPlayer(string playerName)
    {
        var previousPlayer = CurrentPlayerName;
        CurrentPlayerName = playerName;

        _logger.LogDebug("Current player changed from {PreviousPlayer} to {NewPlayer}", previousPlayer, playerName);
    }

    public string GetNextGuessingPlayer()
    {
        if (CurrentGame == null)
        {
            _logger.LogWarning("Attempted to get next guessing player but no game is active");
            return "";
        }

        var guessingPlayers = CurrentGame.Players.Where(p => !p.IsKingPlayer).ToList();
        if (!guessingPlayers.Any())
        {
            _logger.LogWarning("No guessing players found in current game");
            return "";
        }

        var currentIndex = guessingPlayers.FindIndex(p => p.Name == CurrentPlayerName);
        var nextIndex = (currentIndex + 1) % guessingPlayers.Count;
        var nextPlayer = guessingPlayers[nextIndex].Name;

        _logger.LogDebug("Next guessing player: {NextPlayer} (current: {CurrentPlayer})", nextPlayer, CurrentPlayerName);

        return nextPlayer;
    }

    public List<string> GetGuessingPlayers()
    {
        if (CurrentGame == null)
        {
            _logger.LogWarning("Attempted to get guessing players but no game is active");
            return new List<string>();
        }

        var guessingPlayers = CurrentGame.Players.Where(p => !p.IsKingPlayer).Select(p => p.Name).ToList();
        _logger.LogDebug("Retrieved {Count} guessing players: {Players}", guessingPlayers.Count, string.Join(", ", guessingPlayers));

        return guessingPlayers;
    }

    public bool AllGuessingPlayersAnswered(int roundIndex)
    {
        if (CurrentGame == null || roundIndex >= CurrentGame.Questions.Count)
        {
            _logger.LogWarning("Cannot check if all players answered - Game: {HasGame}, Round: {RoundIndex}, Total Questions: {QuestionCount}",
                CurrentGame != null, roundIndex, CurrentGame?.Questions.Count ?? 0);
            return false;
        }

        var question = CurrentGame.Questions[roundIndex];
        var guessingPlayers = CurrentGame.Players.Where(p => !p.IsKingPlayer);
        var allAnswered = guessingPlayers.All(p => question.HasPlayerAnswered(p.Name));

        _logger.LogDebug("Round {RoundIndex} - All guessing players answered: {AllAnswered}", roundIndex, allAnswered);

        return allAnswered;
    }
}
