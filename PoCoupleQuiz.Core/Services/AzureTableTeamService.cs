using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using PoCoupleQuiz.Core.Models;
using System.Text.Json;

namespace PoCoupleQuiz.Core.Services;

public class AzureTableTeamService : ITeamService, IAzureTableTeamService
{
    private readonly TableClient _tableClient;

    public AzureTableTeamService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"] ?? "UseDevelopmentStorage=true";
        _tableClient = new TableClient(connectionString, "Teams");
        _tableClient.CreateIfNotExists();
    }

    public async Task<Team?> GetTeamAsync(string teamName)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<TeamTableEntity>(
                partitionKey: "Team",
                rowKey: teamName.ToLowerInvariant()
            );
            return response.Value.ToTeam();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Team>> GetAllTeamsAsync()
    {
        var teams = new List<Team>();
        var queryResults = _tableClient.QueryAsync<TeamTableEntity>(filter: $"PartitionKey eq 'Team'");
        
        await foreach (var entity in queryResults)
        {
            teams.Add(entity.ToTeam());
        }

        return teams;
    }

    public async Task SaveTeamAsync(Team team)
    {
        var entity = new TeamTableEntity(team);
        await _tableClient.UpsertEntityAsync(entity);
    }

    public async Task UpdateTeamStatsAsync(string teamName, GameMode gameMode, int score)
    {
        var team = await GetTeamAsync(teamName);
        if (team == null) return;

        if (gameMode == GameMode.KingPlayer)
        {
            team.MultiplayerWins += score > 0 ? 1 : 0;
        }

        await SaveTeamAsync(team);
    }
}

public class TeamTableEntity : ITableEntity
{
    public TeamTableEntity()
    {
    }

    public TeamTableEntity(Team team)
    {
        PartitionKey = team.PartitionKey;
        RowKey = team.RowKey;
        Name = team.Name;
        MultiplayerWins = team.MultiplayerWins;
        SinglePlayerHighScore = team.SinglePlayerHighScore;
        LastPlayed = team.LastPlayed;
        TotalQuestionsAnswered = team.TotalQuestionsAnswered;
        CorrectAnswers = team.CorrectAnswers;
    }

    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Name { get; set; } = string.Empty;
    public int MultiplayerWins { get; set; }
    public int SinglePlayerHighScore { get; set; }
    public DateTime LastPlayed { get; set; }
    public int TotalQuestionsAnswered { get; set; }
    public int CorrectAnswers { get; set; }

    public Team ToTeam()
    {
        return new Team
        {
            Name = Name,
            MultiplayerWins = MultiplayerWins,
            SinglePlayerHighScore = SinglePlayerHighScore,
            LastPlayed = LastPlayed,
            TotalQuestionsAnswered = TotalQuestionsAnswered,
            CorrectAnswers = CorrectAnswers
        };
    }
}
