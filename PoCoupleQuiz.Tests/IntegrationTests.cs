using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using PoCoupleQuiz.Tests.Utilities; // Added for CustomWebApplicationFactory
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;

namespace PoCoupleQuiz.Tests
{
    public class IntegrationTests : IAsyncLifetime
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _httpClient;

        public IntegrationTests()
        {
            _factory = new CustomWebApplicationFactory();
            _httpClient = _factory.CreateClient();
            _httpClient.BaseAddress = _factory.Server.BaseAddress;
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _httpClient.Dispose();
            _factory.Dispose();
        }

        [Fact]
        public async Task HomePage_LoadsSuccessfully()
        {
            // Act
            var response = await _httpClient.GetAsync("/");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Contains("PoCoupleQuiz", content);
            Assert.Contains("container", content);
        }

        [Fact]
        public async Task GamePage_LoadsSuccessfully()
        {
            // Act
            var response = await _httpClient.GetAsync("/game/king");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Contains("game", content);
        }

        [Fact]
        public async Task LeaderboardPage_LoadsSuccessfully()
        {
            // Act
            var response = await _httpClient.GetAsync("/leaderboard");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Contains("Leaderboard", content);
        }

        [Fact]
        public async Task DiagnosticsPage_LoadsSuccessfully()
        {
            // Act
            var response = await _httpClient.GetAsync("/diag");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Contains("Diagnostics", content);
        }

        [Fact]
        public async Task TeamsApi_WorksCorrectly()
        {
            // Arrange
            var team = new Team
            {
                Name = "TestTeam",
                TotalQuestionsAnswered = 10,
                CorrectAnswers = 8
            };

            var json = JsonSerializer.Serialize(team);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var createResponse = await _httpClient.PostAsync("/api/teams", content);
            createResponse.EnsureSuccessStatusCode();

            var getResponse = await _httpClient.GetAsync("/api/teams");
            var teamsJson = await getResponse.Content.ReadAsStringAsync();
            var teams = JsonSerializer.Deserialize<List<Team>>(teamsJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            Assert.NotNull(teams);
            Assert.Contains(teams, t => t.Name == "TestTeam");
        }

        [Fact]
        public async Task ResponsiveLayoutElements_ArePresent()
        {
            // Act
            var response = await _httpClient.GetAsync("/");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            // Verify responsive design elements are present
            Assert.Contains("container", content);
            Assert.Contains("btn", content);
            Assert.Contains("quiz-card", content);
        }
    }
}
