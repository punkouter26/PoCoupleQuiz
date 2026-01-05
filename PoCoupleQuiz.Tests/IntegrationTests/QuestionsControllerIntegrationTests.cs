 using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using PoCoupleQuiz.Tests.Utilities;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;

namespace PoCoupleQuiz.Tests.IntegrationTests;

/// <summary>
/// Integration tests for QuestionsController API endpoints.
/// Tests POST /api/questions/generate and POST /api/questions/check-similarity.
/// </summary>
[Trait("Category", "Integration")]
public class QuestionsControllerIntegrationTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public QuestionsControllerIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
        _httpClient = _factory.CreateClient();
        _httpClient.BaseAddress = _factory.Server.BaseAddress;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        _httpClient.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GenerateQuestion_Post_ReturnsQuestion()
    {
        // Arrange
        var request = new { Difficulty = "easy" };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/questions/generate", request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(content);

        // Deserialize and validate
        var question = JsonSerializer.Deserialize<Question>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(question);
        Assert.NotEmpty(question.Text);
    }

    [Fact]
    public async Task GenerateQuestion_WithNullDifficulty_ReturnsQuestion()
    {
        // Arrange
        var request = new { Difficulty = (string?)null };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/questions/generate", request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task GenerateQuestion_WithMediumDifficulty_ReturnsQuestion()
    {
        // Arrange
        var request = new { Difficulty = "medium" };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/questions/generate", request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(content);

        var question = JsonSerializer.Deserialize<Question>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(question);
    }

    [Fact]
    public async Task GenerateQuestion_WithHardDifficulty_ReturnsQuestion()
    {
        // Arrange
        var request = new { Difficulty = "hard" };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/questions/generate", request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(content);

        var question = JsonSerializer.Deserialize<Question>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(question);
    }

    [Fact]
    public async Task CheckSimilarity_IdenticalAnswers_ReturnsTrue()
    {
        // Arrange
        var request = new { Answer1 = "Paris", Answer2 = "Paris" };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/questions/check-similarity", request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = JsonSerializer.Deserialize<bool>(content);
        Assert.True(result);
    }

    [Fact]
    public async Task CheckSimilarity_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var request = new { Answer1 = "PARIS", Answer2 = "paris" };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/questions/check-similarity", request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = JsonSerializer.Deserialize<bool>(content);
        Assert.True(result);
    }

    [Fact]
    public async Task CheckSimilarity_DifferentAnswers_ReturnsFalse()
    {
        // Arrange
        var request = new { Answer1 = "Paris", Answer2 = "London" };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/questions/check-similarity", request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = JsonSerializer.Deserialize<bool>(content);
        Assert.False(result);
    }

    [Fact]
    public async Task CheckSimilarity_SimilarAnswers_ReturnsCorrectResult()
    {
        // Arrange - Testing synonyms/similar meanings
        var request = new { Answer1 = "Happy", Answer2 = "Joyful" };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/questions/check-similarity", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Result depends on AI service implementation, just ensure it returns without error
    }

    [Fact]
    public async Task GenerateQuestion_WithEmptyBody_ReturnsQuestion()
    {
        // Arrange - Empty object
        var request = new { };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/questions/generate", request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task GenerateQuestion_ContentTypeIsJson()
    {
        // Arrange
        var request = new { Difficulty = "easy" };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/questions/generate", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }
}
