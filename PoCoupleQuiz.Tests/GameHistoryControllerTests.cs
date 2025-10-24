using Xunit;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using PoCoupleQuiz.Tests.Utilities;
using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Tests;

/// <summary>
/// Integration tests for GameHistoryController endpoints
/// </summary>
public class GameHistoryControllerTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GameHistoryControllerTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task SaveGameHistory_ValidHistory_ReturnsOk()
    {
        // Arrange
        var history = new GameHistory
        {
            Team1Name = "TeamA",
            Team2Name = "TeamB",
            TotalQuestions = 10,
            Team1Score = 7,
            Team2Score = 5,
            GameMode = GameMode.KingPlayer,
            Date = DateTime.UtcNow
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/GameHistory", history);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SaveGameHistory_NullHistory_ReturnsBadRequest()
    {
        // Arrange
        var content = new StringContent("null", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/GameHistory", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SaveGameHistory_MissingTeamNames_ReturnsBadRequest()
    {
        // Arrange
        var history = new GameHistory
        {
            Team1Name = "",
            Team2Name = "",
            TotalQuestions = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/GameHistory", history);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SaveGameHistory_NegativeTotalQuestions_ReturnsBadRequest()
    {
        // Arrange
        var history = new GameHistory
        {
            Team1Name = "TeamA",
            Team2Name = "TeamB",
            TotalQuestions = -1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/GameHistory", history);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTeamHistory_ExistingTeam_ReturnsHistory()
    {
        // Arrange
        var teamName = "TestTeam";
        var history = new GameHistory
        {
            Team1Name = teamName,
            Team2Name = "OpponentTeam",
            TotalQuestions = 5,
            Team1Score = 3,
            Team2Score = 2,
            GameMode = GameMode.KingPlayer
        };
        await _client.PostAsJsonAsync("/api/GameHistory", history);

        // Act
        var response = await _client.GetAsync($"/api/GameHistory/team/{teamName}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<GameHistory>>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetCategoryStats_ExistingTeam_ReturnsStats()
    {
        // Arrange
        var teamName = "CategoryTestTeam";

        // Act
        var response = await _client.GetAsync($"/api/GameHistory/categoryStats/{teamName}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Dictionary<QuestionCategory, int>>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetTopMatchedAnswers_ValidTeamAndCount_ReturnsAnswers()
    {
        // Arrange
        var teamName = "AnswersTestTeam";
        var count = 5;

        // Act
        var response = await _client.GetAsync($"/api/GameHistory/topMatchedAnswers/{teamName}/{count}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAverageResponseTime_ValidTeam_ReturnsTime()
    {
        // Arrange
        var teamName = "TimeTestTeam";

        // Act
        var response = await _client.GetAsync($"/api/GameHistory/averageResponseTime/{teamName}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<double>();
        Assert.True(result >= 0);
    }
}
