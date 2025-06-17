using PoCoupleQuiz.Core.Models;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace PoCoupleQuiz.Core.Services;

public class HttpTeamService : ITeamService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpTeamService> _logger;

    public HttpTeamService(HttpClient httpClient, ILogger<HttpTeamService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }    public async Task<Team?> GetTeamAsync(string teamName)
    {
        try
        {
            _logger.LogInformation("Getting team: {TeamName}", teamName);
            return await _httpClient.GetFromJsonAsync<Team>($"/api/teams/{teamName}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to get team {TeamName}: {Message}", teamName, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting team {TeamName}", teamName);
            throw;
        }
    }

    public async Task<IEnumerable<Team>> GetAllTeamsAsync()
    {
        try
        {
            _logger.LogInformation("Getting all teams");
            var teams = await _httpClient.GetFromJsonAsync<Team[]>("/api/teams");
            return teams ?? Array.Empty<Team>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to get all teams: {Message}", ex.Message);
            return Array.Empty<Team>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting all teams");
            throw;
        }
    }

    public async Task SaveTeamAsync(Team team)
    {
        try
        {
            _logger.LogInformation("Saving team: {TeamName}", team.Name);
            await _httpClient.PostAsJsonAsync("/api/teams", team);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to save team {TeamName}: {Message}", team.Name, ex.Message);
            throw new InvalidOperationException($"Failed to save team {team.Name}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error saving team {TeamName}", team.Name);
            throw;
        }
    }

    public async Task UpdateTeamStatsAsync(string teamName, GameMode gameMode, int score)
    {
        try
        {
            _logger.LogInformation("Updating team stats for {TeamName}: Mode={GameMode}, Score={Score}", teamName, gameMode, score);
            await _httpClient.PutAsJsonAsync($"/api/teams/{teamName}/stats", new { gameMode, score });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to update team stats for {TeamName}: {Message}", teamName, ex.Message);
            throw new InvalidOperationException($"Failed to update team stats for {teamName}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating team stats for {TeamName}", teamName);
            throw;
        }
    }
}
