using PoCoupleQuiz.Core.Models;
using System.Net.Http.Json;

namespace PoCoupleQuiz.Core.Services;

public class HttpTeamService : ITeamService
{
    private readonly HttpClient _httpClient;

    public HttpTeamService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Team?> GetTeamAsync(string teamName)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Team>($"/api/teams/{teamName}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Team>> GetAllTeamsAsync()
    {
        try
        {
            var teams = await _httpClient.GetFromJsonAsync<Team[]>("/api/teams");
            return teams ?? Array.Empty<Team>();
        }
        catch (HttpRequestException)
        {
            return Array.Empty<Team>();
        }
    }

    public async Task SaveTeamAsync(Team team)
    {
        try
        {
            await _httpClient.PostAsJsonAsync("/api/teams", team);
        }
        catch (HttpRequestException)
        {
            // Handle error appropriately
        }
    }

    public async Task UpdateTeamStatsAsync(string teamName, GameMode gameMode, int score)
    {
        try
        {
            await _httpClient.PutAsJsonAsync($"/api/teams/{teamName}/stats", new { gameMode, score });
        }
        catch (HttpRequestException)
        {
            // Handle error appropriately
        }
    }
}
