using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

public class AzureTableTeamService : ITeamService
{
    private readonly TableClient _tableClient;
    private readonly ILogger<AzureTableTeamService> _logger;

    public AzureTableTeamService(IConfiguration configuration, ILogger<AzureTableTeamService> logger)
    {
        _logger = logger;

        var connectionString = configuration["AzureTableStorage:ConnectionString"]
            ?? throw new ArgumentNullException("AzureTableStorage:ConnectionString", "Azure Storage connection string is required");

        _tableClient = new TableClient(connectionString, "Teams");
        _tableClient.CreateIfNotExists();

        _logger.LogInformation("AzureTableTeamService initialized successfully");
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team {TeamName}: {ErrorMessage}", teamName, ex.Message);
            throw;
        }
    }
    public async Task<IEnumerable<Team>> GetAllTeamsAsync()
    {
        try
        {
            var teams = new List<Team>();
            var queryResults = _tableClient.QueryAsync<TeamTableEntity>(filter: $"PartitionKey eq 'Team'");

            await foreach (var entity in queryResults)
            {
                teams.Add(entity.ToTeam());
            }

            return teams;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all teams: {ErrorMessage}", ex.Message);
            throw;
        }
    }
    public async Task SaveTeamAsync(Team team)
    {
        try
        {
            var entity = new TeamTableEntity(team);
            await _tableClient.UpsertEntityAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving team {TeamName}: {ErrorMessage}", team.Name, ex.Message);
            throw;
        }
    }
    public async Task UpdateTeamStatsAsync(string teamName, GameMode gameMode, int score, int questionsAnswered = 0, int correctAnswers = 0)
    {
        var team = await GetTeamAsync(teamName);
        
        // Create team if it doesn't exist
        if (team == null)
        {
            team = new Team
            {
                Name = teamName,
                HighScore = 0,
                TotalQuestionsAnswered = 0,
                CorrectAnswers = 0,
                LastPlayed = DateTime.UtcNow
            };
        }

        // Update high score if applicable
        if (gameMode == GameMode.KingPlayer)
        {
            if (score > team.HighScore)
            {
                team.HighScore = score;
            }
        }

        // Update statistics
        team.TotalQuestionsAnswered += questionsAnswered;
        team.CorrectAnswers += correctAnswers;
        team.LastPlayed = DateTime.UtcNow;

        await SaveTeamAsync(team);
        
        _logger.LogInformation("Updated stats for team {TeamName}: HighScore={HighScore}, Score={Score}, TotalQuestions={Total}, Correct={Correct}", 
            teamName, team.HighScore, score, team.TotalQuestionsAnswered, team.CorrectAnswers);
    }
}

public class TeamTableEntity : ITableEntity
{
    public TeamTableEntity()
    {
    }
    public TeamTableEntity(Team team)
    {
        PartitionKey = "Team";
        RowKey = team.Name.ToLowerInvariant();
        Name = team.Name;
        HighScore = team.HighScore;
        LastPlayed = team.LastPlayed;
        TotalQuestionsAnswered = team.TotalQuestionsAnswered;
        CorrectAnswers = team.CorrectAnswers;
    }

    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string Name { get; set; } = string.Empty;
    public int HighScore { get; set; }
    public DateTime LastPlayed { get; set; }
    public int TotalQuestionsAnswered { get; set; }
    public int CorrectAnswers { get; set; }

    public Team ToTeam()
    {
        return new Team
        {
            Name = Name,
            HighScore = HighScore,
            LastPlayed = LastPlayed,
            TotalQuestionsAnswered = TotalQuestionsAnswered,
            CorrectAnswers = CorrectAnswers
        };
    }
}
