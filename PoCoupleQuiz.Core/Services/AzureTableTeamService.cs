using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

public class AzureTableTeamService : ITeamService
{
    private readonly TableClient _tableClient;

    public AzureTableTeamService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"] ?? throw new ArgumentNullException("AzureStorage:ConnectionString");
        _tableClient = new TableClient(connectionString, "Teams");
        _tableClient.CreateIfNotExists();
    }

    public async Task<Team?> GetTeamAsync(string teamName)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<TableEntity>("Team", teamName.ToLowerInvariant());
            var entity = response.Value;

            return new Team
            {
                Name = entity.GetString("Name"),
                MultiplayerWins = entity.GetInt32("MultiplayerWins") ?? 0,
                SinglePlayerHighScore = entity.GetInt32("SinglePlayerHighScore") ?? 0,
                LastPlayed = entity.GetDateTime("LastPlayed") ?? DateTime.UtcNow
            };
        }
        catch (RequestFailedException)
        {
            return null;
        }
    }

    public async Task SaveTeamAsync(Team team)
    {
        // Ensure LastPlayed is UTC
        if (team.LastPlayed.Kind != DateTimeKind.Utc)
        {
            team.LastPlayed = DateTime.SpecifyKind(team.LastPlayed, DateTimeKind.Utc);
        }

        var entity = new TableEntity("Team", team.Name.ToLowerInvariant())
        {
            { "Name", team.Name },
            { "MultiplayerWins", team.MultiplayerWins },
            { "SinglePlayerHighScore", team.SinglePlayerHighScore },
            { "LastPlayed", team.LastPlayed }
        };

        await _tableClient.UpsertEntityAsync(entity);
    }

    public async Task UpdateTeamStatsAsync(string teamName, GameMode mode, int score)
    {
        var team = await GetTeamAsync(teamName);
        if (team == null) return;

        if (mode == GameMode.MultiPlayer)
        {
            team.MultiplayerWins += score > 0 ? 1 : 0;
        }
        else
        {
            if (score > team.SinglePlayerHighScore)
            {
                team.SinglePlayerHighScore = score;
            }
        }
        
        team.LastPlayed = DateTime.UtcNow;
        await SaveTeamAsync(team);
    }
}