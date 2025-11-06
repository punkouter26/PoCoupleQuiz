using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;
using System.Collections.Concurrent;

namespace PoCoupleQuiz.Tests.Utilities;

public class InMemoryGameHistoryService : IGameHistoryService
{
    private static readonly ConcurrentBag<GameHistory> _gameHistories = new();

    public Task SaveGameHistoryAsync(GameHistory history)
    {
        history.PartitionKey = "GameHistory";
        history.RowKey = GameHistory.GenerateRowKey();
        history.Timestamp = DateTimeOffset.UtcNow;
        _gameHistories.Add(history);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<GameHistory>> GetTeamHistoryAsync(string teamName)
    {
        var histories = _gameHistories
            .Where(h => h.Team1Name == teamName || h.Team2Name == teamName)
            .OrderByDescending(h => h.Timestamp)
            .ToList();

        return Task.FromResult<IEnumerable<GameHistory>>(histories);
    }

    public Task<Dictionary<QuestionCategory, int>> GetTeamCategoryStatsAsync(string teamName)
    {
        var categoryStats = new Dictionary<QuestionCategory, int>();

        var histories = _gameHistories
            .Where(h => h.Team1Name == teamName || h.Team2Name == teamName);

        foreach (var history in histories)
        {
            if (!string.IsNullOrEmpty(history.CategoryStats))
            {
                try
                {
                    var stats = System.Text.Json.JsonSerializer.Deserialize<Dictionary<QuestionCategory, int>>(history.CategoryStats);
                    if (stats != null)
                    {
                        foreach (var (category, count) in stats)
                        {
                            if (!categoryStats.ContainsKey(category))
                                categoryStats[category] = 0;
                            categoryStats[category] += count;
                        }
                    }
                }
                catch
                {
                    // Ignore invalid JSON
                }
            }
        }

        return Task.FromResult(categoryStats);
    }

    public Task<List<string>> GetTopMatchedAnswersAsync(string teamName, int count = 10)
    {
        var answerCounts = new Dictionary<string, int>();

        var histories = _gameHistories
            .Where(h => h.Team1Name == teamName || h.Team2Name == teamName);

        foreach (var history in histories)
        {
            if (!string.IsNullOrEmpty(history.MatchedAnswers))
            {
                try
                {
                    var answers = System.Text.Json.JsonSerializer.Deserialize<List<string>>(history.MatchedAnswers);
                    if (answers != null)
                    {
                        foreach (var answer in answers)
                        {
                            if (!answerCounts.ContainsKey(answer))
                                answerCounts[answer] = 0;
                            answerCounts[answer]++;
                        }
                    }
                }
                catch
                {
                    // Ignore invalid JSON
                }
            }
        }

        var topAnswers = answerCounts
            .OrderByDescending(x => x.Value)
            .Take(count)
            .Select(x => x.Key)
            .ToList();

        return Task.FromResult(topAnswers);
    }

    public Task<double> GetAverageResponseTimeAsync(string teamName)
    {
        var histories = _gameHistories
            .Where(h => h.Team1Name == teamName || h.Team2Name == teamName)
            .ToList();

        if (!histories.Any())
            return Task.FromResult(0.0);

        var averageTime = histories.Average(h => h.AverageResponseTime);
        return Task.FromResult(averageTime);
    }
}
