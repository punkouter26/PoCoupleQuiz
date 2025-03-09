using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using PoCoupleQuiz.Core.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace PoCoupleQuiz.Core.Services;

public class AzureTableTeamService : ITeamService, IAzureTableTeamService
{
    private readonly TableClient _tableClient;
    private readonly IConfiguration _configuration;
    // Fallback in-memory storage when Azure Storage is not available
    private static readonly ConcurrentDictionary<string, Team> _inMemoryTeams = new();
    private readonly bool _useInMemoryFallback;

    public AzureTableTeamService(IConfiguration configuration)
    {
        _configuration = configuration;
        _useInMemoryFallback = false;

        try
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Azure Storage connection string is required");
            }

            _tableClient = new TableClient(connectionString, "Teams");
            _tableClient.CreateIfNotExists();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize Azure Table Storage: {ex.Message}");
            _useInMemoryFallback = true;
            Console.WriteLine("Falling back to in-memory storage for teams.");
            throw; // Rethrow to ensure the application knows there's a configuration issue
        }
    }

    public async Task<Team?> GetTeamAsync(string teamName)
    {
        if (_useInMemoryFallback)
        {
            return _inMemoryTeams.TryGetValue(teamName.ToLowerInvariant(), out var team) ? team : null;
        }

        try
        {
            var response = await _tableClient!.GetEntityAsync<TeamTableEntity>(
                partitionKey: "Team",
                rowKey: teamName.ToLowerInvariant()
            );
            return response.Value.ToTeam();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Azure Table Storage error: {ex.Message}");
            // Fallback to in-memory if Azure storage fails
            return _inMemoryTeams.TryGetValue(teamName.ToLowerInvariant(), out var team) ? team : null;
        }
    }

    public async Task<IEnumerable<Team>> GetAllTeamsAsync()
    {
        if (_useInMemoryFallback)
        {
            return _inMemoryTeams.Values.ToList();
        }

        try
        {
            var teams = new List<Team>();
            var queryResults = _tableClient!.QueryAsync<TeamTableEntity>(filter: $"PartitionKey eq 'Team'");

            await foreach (var entity in queryResults)
            {
                teams.Add(entity.ToTeam());
            }

            return teams;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Azure Table Storage error: {ex.Message}");
            // Fallback to in-memory if Azure storage fails
            return _inMemoryTeams.Values.ToList();
        }
    }

    public async Task SaveTeamAsync(Team team)
    {
        if (_useInMemoryFallback)
        {
            _inMemoryTeams[team.Name.ToLowerInvariant()] = team;
            return;
        }

        try
        {
            var entity = new TeamTableEntity(team);
            await _tableClient!.UpsertEntityAsync(entity);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Azure Table Storage error: {ex.Message}");
            // Fallback to in-memory if Azure storage fails
            _inMemoryTeams[team.Name.ToLowerInvariant()] = team;
        }
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
