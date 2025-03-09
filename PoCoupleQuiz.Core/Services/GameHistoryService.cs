using System.Text.Json;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

public class GameHistoryService : IGameHistoryService
{
    private readonly TableClient _tableClient;

    public GameHistoryService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"] ?? throw new ArgumentNullException("AzureStorage:ConnectionString");
        _tableClient = new TableClient(connectionString, "GameHistory");
        _tableClient.CreateIfNotExists();
    }

    public async Task SaveGameHistoryAsync(GameHistory history)
    {
        history.PartitionKey = "GameHistory";
        history.RowKey = GameHistory.GenerateRowKey();
        await _tableClient.AddEntityAsync(history);
    }

    public async Task<IEnumerable<GameHistory>> GetTeamHistoryAsync(string teamName)
    {
        var histories = new List<GameHistory>();
        var filter = $"(Team1Name eq '{teamName}' or Team2Name eq '{teamName}')";
        var queryResults = _tableClient.QueryAsync<GameHistory>(filter);

        await foreach (var history in queryResults)
        {
            histories.Add(history);
        }

        return histories.OrderByDescending(h => h.Timestamp);
    }

    public async Task<Dictionary<QuestionCategory, int>> GetTeamCategoryStatsAsync(string teamName)
    {
        var histories = await GetTeamHistoryAsync(teamName);
        var categoryStats = new Dictionary<QuestionCategory, int>();

        foreach (var history in histories)
        {
            var stats = JsonSerializer.Deserialize<Dictionary<QuestionCategory, int>>(history.CategoryStats);
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

        return categoryStats;
    }

    public async Task<List<string>> GetTopMatchedAnswersAsync(string teamName, int count = 10)
    {
        var histories = await GetTeamHistoryAsync(teamName);
        var answerCounts = new Dictionary<string, int>();

        foreach (var history in histories)
        {
            var answers = JsonSerializer.Deserialize<List<string>>(history.MatchedAnswers);
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

        return answerCounts
            .OrderByDescending(x => x.Value)
            .Take(count)
            .Select(x => x.Key)
            .ToList();
    }

    public async Task<double> GetAverageResponseTimeAsync(string teamName)
    {
        var histories = await GetTeamHistoryAsync(teamName);
        if (!histories.Any()) return 0;

        return histories.Average(h => h.AverageResponseTime);
    }
}