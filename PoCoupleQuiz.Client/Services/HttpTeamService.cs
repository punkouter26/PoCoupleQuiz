using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace PoCoupleQuiz.Client.Services;

public class HttpTeamService : ITeamService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpTeamService> _logger;

    public HttpTeamService(HttpClient httpClient, ILogger<HttpTeamService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    public async Task<Team?> GetTeamAsync(string teamName)
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
            var response = await _httpClient.PostAsJsonAsync("/api/teams", team);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to save team {TeamName}. Status: {StatusCode}, Error: {Error}", 
                    team.Name, response.StatusCode, errorContent);
                // Don't throw, just log the warning - the game can continue without saving
                return;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to save team {TeamName}: {Message}", team.Name, ex.Message);
            // Don't throw - allow the game to continue even if save fails
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error saving team {TeamName}", team.Name);
            // Don't throw - allow the game to continue even if save fails
        }
    }

    public async Task UpdateTeamStatsAsync(string teamName, int score, int questionsAnswered = 0, int correctAnswers = 0)
    {
        try
        {
            _logger.LogInformation("Updating team stats for {TeamName}: Score={Score}, Questions={Questions}, Correct={Correct}",
                teamName, score, questionsAnswered, correctAnswers);
            var response = await _httpClient.PutAsJsonAsync($"/api/teams/{teamName}/stats", new { score, questionsAnswered, correctAnswers });
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to update team stats for {TeamName}. Status: {StatusCode}, Error: {Error}", 
                    teamName, response.StatusCode, errorContent);
                // Don't throw, just log the warning - the game can continue without updating stats
                return;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to update team stats for {TeamName}: {Message}", teamName, ex.Message);
            // Don't throw - allow the game to continue even if update fails
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating team stats for {TeamName}", teamName);
            // Don't throw - allow the game to continue even if update fails
        }
    }
}
