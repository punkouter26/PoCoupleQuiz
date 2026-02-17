using Xunit;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net;
using PoCoupleQuiz.Tests.Utilities;
using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Tests;

/// <summary>
/// Additional integration tests for TeamsController endpoints
/// </summary>
public class TeamsControllerTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TeamsControllerTests()
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

    [Trait("Category", "Integration")]
    [Fact]
    public async Task UpdateTeamStats_ValidRequest_ReturnsOk()
    {
        // Arrange
        var teamName = "UpdateTestTeam";
        var team = new Team
        {
            Name = teamName,
            TotalQuestionsAnswered = 0,
            CorrectAnswers = 0
        };
        await _client.PostAsJsonAsync("/api/teams", team);

        var updateRequest = new
        {
            Score = 10
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/teams/{teamName}/stats", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Trait("Category", "Integration")]
    [Fact]
    public async Task UpdateTeamStats_EmptyTeamName_ReturnsBadRequest()
    {
        // Arrange
        var updateRequest = new
        {
            Score = 10
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/teams/ /stats", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Trait("Category", "Integration")]
    [Fact]
    public async Task UpdateTeamStats_LongTeamName_ReturnsBadRequest()
    {
        // Arrange
        var longName = new string('A', 101);
        var updateRequest = new
        {
            Score = 10
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/teams/{longName}/stats", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Trait("Category", "Integration")]
    [Fact]
    public async Task UpdateTeamStats_NegativeScore_ReturnsBadRequest()
    {
        // Arrange
        var teamName = "NegativeScoreTeam";
        var updateRequest = new
        {
            Score = -5
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/teams/{teamName}/stats", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Trait("Category", "Integration")]
    [Fact]
    public async Task GetAllTeams_ReturnsTeamsList()
    {
        // Arrange - Create a test team first
        var team = new Team
        {
            Name = "GetAllTestTeam",
            TotalQuestionsAnswered = 5,
            CorrectAnswers = 3
        };
        await _client.PostAsJsonAsync("/api/teams", team);

        // Act
        var response = await _client.GetAsync("/api/teams");

        // Assert
        response.EnsureSuccessStatusCode();
        var teams = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<Team>>();
        Assert.NotNull(teams);
    }

    [Trait("Category", "Integration")]
    [Fact]
    public async Task SaveTeam_ValidTeam_ReturnsOk()
    {
        // Arrange
        var team = new Team
        {
            Name = "NewTestTeam",
            TotalQuestionsAnswered = 10,
            CorrectAnswers = 7
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/teams", team);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Trait("Category", "Integration")]
    [Fact]
    public async Task SaveTeam_NegativeStatistics_ReturnsBadRequest()
    {
        // Arrange
        var team = new Team
        {
            Name = "NegativeStatsTeam",
            TotalQuestionsAnswered = -1,
            CorrectAnswers = 5
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/teams", team);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}