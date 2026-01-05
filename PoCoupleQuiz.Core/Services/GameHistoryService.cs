using System.Text.Json;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

public interface IGameHistoryService
{
    Task SaveGameHistoryAsync(GameHistory history);
    Task<IEnumerable<GameHistory>> GetTeamHistoryAsync(string teamName);
    Task<Dictionary<QuestionCategory, int>> GetTeamCategoryStatsAsync(string teamName);
    Task<List<string>> GetTopMatchedAnswersAsync(string teamName, int count = 10);
    Task<double> GetAverageResponseTimeAsync(string teamName);
}

public class GameHistoryService : IGameHistoryService
{
    private readonly TableClient _tableClient;
    private readonly ILogger<GameHistoryService> _logger;

    public GameHistoryService(TableServiceClient tableServiceClient, ILogger<GameHistoryService> logger)
    {
        _logger = logger;
        _tableClient = tableServiceClient.GetTableClient("GameHistory");
        _tableClient.CreateIfNotExists();

        _logger.LogInformation("GameHistoryService initialized successfully with Aspire TableServiceClient");
    }

    public async Task SaveGameHistoryAsync(GameHistory history)
    {
        try
        {
            history.PartitionKey = "GameHistory";
            history.RowKey = GameHistory.GenerateRowKey();

            _logger.LogInformation("Saving game history for teams {Team1} vs {Team2}, Score: {Score1}-{Score2}",
                history.Team1Name, history.Team2Name, history.Team1Score, history.Team2Score);

            await _tableClient.AddEntityAsync(history);

            _logger.LogDebug("Game history saved successfully with RowKey: {RowKey}", history.RowKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save game history for teams {Team1} vs {Team2}",
                history.Team1Name, history.Team2Name);
            throw;
        }
    }

    public async Task<IEnumerable<GameHistory>> GetTeamHistoryAsync(string teamName)
    {
        try
        {
            _logger.LogDebug("Retrieving history for team: {TeamName}", teamName);

            var histories = new List<GameHistory>();
            var filter = $"(Team1Name eq '{teamName}' or Team2Name eq '{teamName}')";
            var queryResults = _tableClient.QueryAsync<GameHistory>(filter);

            await foreach (var history in queryResults)
            {
                histories.Add(history);
            }

            _logger.LogInformation("Retrieved {Count} history records for team: {TeamName}", histories.Count, teamName);
            return histories.OrderByDescending(h => h.Timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve history for team: {TeamName}", teamName);
            throw;
        }
    }

    public async Task<Dictionary<QuestionCategory, int>> GetTeamCategoryStatsAsync(string teamName)
    {
        try
        {
            _logger.LogDebug("Calculating category stats for team: {TeamName}", teamName);

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

            _logger.LogInformation("Calculated category stats for team {TeamName}: {Stats}",
                teamName, string.Join(", ", categoryStats.Select(kvp => $"{kvp.Key}:{kvp.Value}")));

            return categoryStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate category stats for team: {TeamName}", teamName);
            throw;
        }
    }

    public async Task<List<string>> GetTopMatchedAnswersAsync(string teamName, int count = 10)
    {
        try
        {
            _logger.LogDebug("Getting top {Count} matched answers for team: {TeamName}", count, teamName);

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

            var topAnswers = answerCounts
                .OrderByDescending(x => x.Value)
                .Take(count)
                .Select(x => x.Key)
                .ToList();

            _logger.LogInformation("Retrieved {Count} top matched answers for team: {TeamName}", topAnswers.Count, teamName);
            return topAnswers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get top matched answers for team: {TeamName}", teamName);
            throw;
        }
    }

    public async Task<double> GetAverageResponseTimeAsync(string teamName)
    {
        try
        {
            _logger.LogDebug("Calculating average response time for team: {TeamName}", teamName);

            var histories = await GetTeamHistoryAsync(teamName);
            if (!histories.Any())
            {
                _logger.LogWarning("No history found for team: {TeamName}, returning 0 average response time", teamName);
                return 0;
            }

            var averageTime = histories.Average(h => h.AverageResponseTime);

            _logger.LogInformation("Average response time for team {TeamName}: {AverageTime:F2}ms", teamName, averageTime);
            return averageTime;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate average response time for team: {TeamName}", teamName);
            throw;
        }
    }
}