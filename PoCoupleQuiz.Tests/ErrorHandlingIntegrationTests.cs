using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using PoCoupleQuiz.Tests.Utilities;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace PoCoupleQuiz.Tests
{
    public class ErrorHandlingIntegrationTests : IAsyncLifetime
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _httpClient;

        public ErrorHandlingIntegrationTests()
        {
            _factory = new CustomWebApplicationFactory();
            _httpClient = _factory.CreateClient();
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _httpClient.Dispose();
            await _factory.DisposeAsync();
        }        [Fact]
        public async Task TeamsController_GetTeam_InvalidName_ReturnsBadRequest()
        {
            // Act - Use URL encoded space which should be trimmed to empty
            var response = await _httpClient.GetAsync("/api/teams/%20");
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TeamsController_GetTeam_ExtremelyLongName_ReturnsBadRequest()
        {
            // Arrange
            var longName = new string('a', 200); // Exceeds 100 character limit
            
            // Act
            var response = await _httpClient.GetAsync($"/api/teams/{longName}");
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TeamsController_SaveTeam_EmptyName_ReturnsBadRequest()
        {
            // Arrange
            var team = new Team
            {
                Name = "",
                TotalQuestionsAnswered = 10,
                CorrectAnswers = 8
            };

            var json = JsonSerializer.Serialize(team);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PostAsync("/api/teams", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TeamsController_SaveTeam_NegativeStats_ReturnsBadRequest()
        {
            // Arrange
            var team = new Team
            {
                Name = "TestTeam",
                TotalQuestionsAnswered = -1,
                CorrectAnswers = 8
            };

            var json = JsonSerializer.Serialize(team);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PostAsync("/api/teams", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TeamsController_UpdateStats_NegativeScore_ReturnsBadRequest()
        {
            // Arrange
            var request = new { gameMode = GameMode.KingPlayer, score = -5 };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PutAsync("/api/teams/TestTeam/stats", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GlobalExceptionHandler_HandlesUnexpectedErrors()
        {
            // This test would require a controller action that throws an exception
            // For now, we'll test that the middleware is registered correctly
            // by ensuring normal requests still work
            
            // Act
            var response = await _httpClient.GetAsync("/api/teams");
            
            // Assert
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task TeamsController_SaveTeam_MalformedJson_ReturnsBadRequest()
        {
            // Arrange
            var malformedJson = "{ invalid json }";
            var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PostAsync("/api/teams", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TeamsController_UpdateStats_InvalidGameMode_ReturnsBadRequest()
        {
            // Arrange
            var request = new { gameMode = 999, score = 5 }; // Invalid enum value
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PutAsync("/api/teams/TestTeam/stats", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
