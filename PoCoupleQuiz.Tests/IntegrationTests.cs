using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace PoCoupleQuiz.Tests
{
    public class IntegrationTests : IAsyncLifetime
    {
        private readonly HttpClient _httpClient;
        private HubConnection? _hubConnection1;
        private HubConnection? _hubConnection2;
        private string _roomId = string.Empty;
        private List<string> _receivedMessages1 = new();
        private List<string> _receivedMessages2 = new();

        public IntegrationTests()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5295") // Using the HTTP port from the running server
            };
        }

        public async Task InitializeAsync()
        {
            _receivedMessages1 = new List<string>();
            _receivedMessages2 = new List<string>();

            // Create two hub connections to simulate two players
            _hubConnection1 = new HubConnectionBuilder()
                .WithUrl("http://localhost:5295/gamehub") // Using the HTTP port from the running server
                .WithAutomaticReconnect()
                .Build();

            _hubConnection2 = new HubConnectionBuilder()
                .WithUrl("http://localhost:5295/gamehub") // Using the HTTP port from the running server
                .WithAutomaticReconnect()
                .Build();

            // Set up message handlers
            _hubConnection1.On<string>("PlayerJoined", (name) =>
            {
                _receivedMessages1.Add($"PlayerJoined: {name}");
            });

            _hubConnection2.On<string>("PlayerJoined", (name) =>
            {
                _receivedMessages2.Add($"PlayerJoined: {name}");
            });

            _hubConnection1.On<string>("PlayerLeft", (name) =>
            {
                _receivedMessages1.Add($"PlayerLeft: {name}");
            });

            _hubConnection2.On<string>("PlayerLeft", (name) =>
            {
                _receivedMessages2.Add($"PlayerLeft: {name}");
            });

            _hubConnection1.On("GameStarted", () =>
            {
                _receivedMessages1.Add("GameStarted");
            });

            _hubConnection2.On("GameStarted", () =>
            {
                _receivedMessages2.Add("GameStarted");
            });

            // Start connections
            await _hubConnection1.StartAsync();
            await _hubConnection2.StartAsync();

            // Generate a test room ID
            _roomId = "test123";
        }

        public async Task DisposeAsync()
        {
            if (_hubConnection1 != null)
                await _hubConnection1.DisposeAsync();
            if (_hubConnection2 != null)
                await _hubConnection2.DisposeAsync();
            _httpClient.Dispose();
        }

        [Fact]
        public async Task MultiplayerGameIsResponsive()
        {
            // Arrange
            string player1 = "Player1";
            string player2 = "Player2";

            // Act
            // 1. Join room and verify responsive layout
            if (_hubConnection1 != null && _hubConnection2 != null)
            {
                await _hubConnection1.SendAsync("JoinRoom", _roomId, player1);
                await _hubConnection2.SendAsync("JoinRoom", _roomId, player2);

                // Get the multiplayer page content
                var response = await _httpClient.GetAsync("/multiplayer");
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                // Verify responsive classes are present
                Assert.Contains("container", content);
                Assert.Contains("quiz-card", content);
                Assert.Contains("multiplayer-room", content);
                Assert.Contains("player-card", content);

                // Verify players are connected
                Assert.Contains($"PlayerJoined: {player1}", _receivedMessages1);
                Assert.Contains($"PlayerJoined: {player2}", _receivedMessages1);
            }
            else
            {
                Assert.Fail("Hub connections were not properly initialized");
            }
        }

        [Fact]
        public async Task GameStateUpdatesAreResponsive()
        {
            // Arrange
            string player1 = "Player1";
            string player2 = "Player2";

            // Act
            if (_hubConnection1 != null && _hubConnection2 != null)
            {
                // 1. Join room
                await _hubConnection1.SendAsync("JoinRoom", _roomId, player1);
                await _hubConnection2.SendAsync("JoinRoom", _roomId, player2);

                // 2. Start game
                await _hubConnection1.SendAsync("StartGame", _roomId);

                // 3. Submit answers
                await _hubConnection1.SendAsync("SubmitAnswer", _roomId, player1, 1, "Answer1");
                await _hubConnection2.SendAsync("SubmitAnswer", _roomId, player2, 1, "Answer2");

                // Get the game state
                var response = await _httpClient.GetAsync($"/api/game/{_roomId}");
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                // Verify game state is updated and responsive
                Assert.Contains("game-area", content);
                Assert.Contains("GameStarted", _receivedMessages1);
                Assert.Contains("GameStarted", _receivedMessages2);
            }
            else
            {
                Assert.Fail("Hub connections were not properly initialized");
            }
        }

        [Fact]
        public async Task MobileNavigationWorksWithMultiplayer()
        {
            // Arrange
            string player1 = "Player1";

            // Act
            if (_hubConnection1 != null)
            {
                // 1. Join room
                await _hubConnection1.SendAsync("JoinRoom", _roomId, player1);

                // 2. Navigate through different views
                var responses = new List<HttpResponseMessage>
                {
                    await _httpClient.GetAsync("/multiplayer"),
                    await _httpClient.GetAsync($"/multiplayer/{_roomId}"),
                    await _httpClient.GetAsync("/")
                };

                // Assert
                foreach (var response in responses)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // Verify responsive navigation elements
                    Assert.Contains("container", content);
                    Assert.Contains("btn", content);
                }
            }
            else
            {
                Assert.Fail("Hub connections were not properly initialized");
            }
        }

        [Fact]
        public async Task ResponsiveLayoutUpdatesWithPlayerCount()
        {
            // Arrange
            string player1 = "Player1";
            string player2 = "Player2";
            string player3 = "Player3"; // Unused variable, but keeping for clarity

            // Act
            if (_hubConnection1 != null && _hubConnection2 != null)
            {
                // 1. Join with first player
                await _hubConnection1.SendAsync("JoinRoom", _roomId, player1);

                // Get initial layout
                var initialResponse = await _httpClient.GetAsync($"/multiplayer/{_roomId}");
                var initialContent = await initialResponse.Content.ReadAsStringAsync();

                // 2. Join with second player
                await _hubConnection2.SendAsync("JoinRoom", _roomId, player2);

                // Get updated layout
                var updatedResponse = await _httpClient.GetAsync($"/multiplayer/{_roomId}");
                var updatedContent = await updatedResponse.Content.ReadAsStringAsync();

                // Assert
                // Verify layout adapts to player count
                Assert.Contains("player-card", initialContent);
                Assert.Contains("player-card", updatedContent);
                Assert.Contains($"PlayerJoined: {player1}", _receivedMessages1);
                Assert.Contains($"PlayerJoined: {player2}", _receivedMessages1);
            }
            else
            {
                Assert.Fail("Hub connections were not properly initialized");
            }
        }

        [Fact]
        public async Task ResponsiveErrorHandling()
        {
            // Arrange
            string player1 = "Player1";

            // Act
            if (_hubConnection1 != null)
            {
                // 1. Try to join invalid room
                await _hubConnection1.SendAsync("JoinRoom", "invalid-room", player1);

                // 2. Get error page
                var response = await _httpClient.GetAsync("/multiplayer/invalid-room");
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                // Verify error handling is responsive
                Assert.Contains("container", content);
                Assert.Contains("quiz-card", content);
            }
            else
            {
                Assert.Fail("Hub connections were not properly initialized");
            }
        }
    }
}
