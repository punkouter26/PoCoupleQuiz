using System.Net.Http.Json;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;

namespace PoCoupleQuiz.Client.Services;

public class HttpGameHistoryService : IGameHistoryService
{
    private readonly HttpClient _httpClient;

    public HttpGameHistoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SaveGameHistoryAsync(GameHistory history)
    {
        await _httpClient.PostAsJsonAsync("api/GameHistory", history);
    }

    public async Task<IEnumerable<GameHistory>> GetTeamHistoryAsync(string teamName)
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<GameHistory>>($"api/GameHistory/team/{teamName}") ?? new List<GameHistory>();
    }

    public async Task<Dictionary<QuestionCategory, int>> GetTeamCategoryStatsAsync(string teamName)
    {
        return await _httpClient.GetFromJsonAsync<Dictionary<QuestionCategory, int>>($"api/GameHistory/categoryStats/{teamName}") ?? new Dictionary<QuestionCategory, int>();
    }

    public async Task<List<string>> GetTopMatchedAnswersAsync(string teamName, int count = 10)
    {
        return await _httpClient.GetFromJsonAsync<List<string>>($"api/GameHistory/topMatchedAnswers/{teamName}/{count}") ?? new List<string>();
    }

    public async Task<double> GetAverageResponseTimeAsync(string teamName)
    {
        return await _httpClient.GetFromJsonAsync<double>($"api/GameHistory/averageResponseTime/{teamName}");
    }
}
