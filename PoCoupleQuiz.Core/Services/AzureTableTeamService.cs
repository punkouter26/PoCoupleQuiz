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
                Wins = entity.GetInt32("Wins") ?? 0,
                Losses = entity.GetInt32("Losses") ?? 0
            };
        }
        catch (RequestFailedException)
        {
            return null;
        }
    }

    public async Task SaveTeamAsync(Team team)
    {
        var entity = new TableEntity("Team", team.Name.ToLowerInvariant())
        {
            { "Name", team.Name },
            { "Wins", team.Wins },
            { "Losses", team.Losses }
        };

        await _tableClient.UpsertEntityAsync(entity);
    }

    public async Task UpdateTeamStatsAsync(string teamName, bool won)
    {
        var team = await GetTeamAsync(teamName);
        if (team == null)
        {
            throw new ArgumentException($"Team {teamName} not found");
        }

        if (won)
        {
            team.Wins++;
        }
        else
        {
            team.Losses++;
        }

        await SaveTeamAsync(team);
    }
} 