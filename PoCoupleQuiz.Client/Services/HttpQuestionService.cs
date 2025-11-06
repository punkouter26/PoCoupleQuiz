using System.Net.Http.Json;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;

namespace PoCoupleQuiz.Client.Services;

public class HttpQuestionService : IQuestionService
{
    private readonly HttpClient _httpClient;

    public HttpQuestionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Question> GenerateQuestionAsync(string? difficulty = null)
    {
        var response = await _httpClient.PostAsJsonAsync("api/questions/generate", new { difficulty });
        response.EnsureSuccessStatusCode();
        
        var question = await response.Content.ReadFromJsonAsync<Question>();
        return question ?? throw new InvalidOperationException("Failed to generate question");
    }

    public async Task<bool> CheckAnswerSimilarityAsync(string answer1, string answer2)
    {
        var response = await _httpClient.PostAsJsonAsync("api/questions/check-similarity", new { answer1, answer2 });
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<bool>();
        return result;
    }

    public async Task<string> GenerateAnswerAsync(string question)
    {
        var response = await _httpClient.PostAsJsonAsync("api/questions/generate-answer", new { question });
        response.EnsureSuccessStatusCode();
        
        var answer = await response.Content.ReadAsStringAsync();
        return answer.Trim('"'); // Remove JSON quotes
    }
}
