using Microsoft.Extensions.Logging;
using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

/// <summary>
/// Service for evaluating answers and calculating scores.
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
    int CalculateRoundScore(bool isMatch, GameMode gameMode);
}

public class GameScoringService : IGameScoringService
{
    private readonly ILogger<GameScoringService> _logger;

    public GameScoringService(ILogger<GameScoringService> logger)
    {
        _logger = logger;
    }

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
                    player.Score += 10; // Standard 10 points for correct match

                    _logger.LogInformation(
                        "Player {PlayerName} - Answer: {Answer}, Match: {IsMatch}, Score: +10",
                        playerAnswer.Key, playerAnswer.Value, isSimilar);
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

    public int CalculateRoundScore(bool isMatch, GameMode gameMode = GameMode.KingPlayer)
    {
        // Simple scoring: 10 points for a match, 0 for no match
        return isMatch ? 10 : 0;
    }
}
