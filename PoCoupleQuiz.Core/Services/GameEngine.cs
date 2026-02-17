using Microsoft.Extensions.Logging;
using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

/// <summary>
/// Manages player turn progression and game state.
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

/// <summary>
/// Evaluates answers and calculates game scores.
/// </summary>
public interface IGameScoringService
{
    /// <summary>
    /// Evaluates player answers against the king's answer and calculates scores.
    /// </summary>
    Task<Dictionary<string, bool>> EvaluateAnswersAsync(
        Game game,
        GameQuestion question,
        IQuestionService questionService);

    /// <summary>
    /// Calculates the round score for a specific player.
    /// </summary>
    int CalculateRoundScore(bool isMatch);
}

/// <summary>
/// Unified game engine for managing turn progression, scoring, and game state.
/// Consolidates IGameTurnManager and IGameScoringService functionality.
/// </summary>
public interface IGameEngine
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

    /// <summary>
    /// Evaluates player answers against the king's answer and calculates scores.
    /// </summary>
    Task<Dictionary<string, bool>> EvaluateAnswersAsync(
        Game game,
        GameQuestion question,
        IQuestionService questionService);

    /// <summary>
    /// Calculates the round score for a specific player.
    /// </summary>
    int CalculateRoundScore(bool isMatch);
}

public class GameEngine : IGameEngine, IGameTurnManager, IGameScoringService
{
    /// <summary>
    /// Points awarded per correct answer.
    /// </summary>
    public const int PointsPerCorrectAnswer = 10;

    private readonly ILogger<GameEngine> _logger;

    public GameEngine(ILogger<GameEngine> logger)
    {
        _logger = logger;
    }

    #region Turn Management

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

    #endregion

    #region Scoring

    public async Task<Dictionary<string, bool>> EvaluateAnswersAsync(
        Game game,
        GameQuestion question,
        IQuestionService questionService)
    {
        var matchResults = new Dictionary<string, bool>();

        if (string.IsNullOrEmpty(question.KingPlayerAnswer))
        {
            _logger.LogWarning("Cannot evaluate answers - king player has not answered");
            return matchResults;
        }

        foreach (var playerAnswer in question.PlayerAnswers)
        {
            try
            {
                var isSimilar = await questionService.CheckAnswerSimilarityAsync(
                    question.KingPlayerAnswer,
                    playerAnswer.Value);

                matchResults[playerAnswer.Key] = isSimilar;

                // Update player score
                var player = game.Players.FirstOrDefault(p => p.Name == playerAnswer.Key);
                if (player != null && isSimilar)
                {
                    player.Score += PointsPerCorrectAnswer;

                    _logger.LogInformation(
                        "Player {PlayerName} - Answer: {Answer}, Match: {IsMatch}, Score: +{Points}",
                        playerAnswer.Key, playerAnswer.Value, isSimilar, PointsPerCorrectAnswer);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating answer for player {PlayerName}", playerAnswer.Key);
                matchResults[playerAnswer.Key] = false;
            }
        }

        return matchResults;
    }

    public int CalculateRoundScore(bool isMatch)
    {
        // Simple scoring: 10 points for a match, 0 for no match
        return isMatch ? 10 : 0;
    }

    #endregion
}
