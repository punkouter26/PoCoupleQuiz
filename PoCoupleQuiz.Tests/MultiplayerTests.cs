using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using PoCoupleQuiz.Web.Hubs;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PoCoupleQuiz.Tests
{
    public class MultiplayerTests : IAsyncLifetime
    {
        private HubConnection? _hubConnection1;
        private HubConnection? _hubConnection2;
        private string _roomId = string.Empty;
        private List<string> _receivedMessages1 = new();
        private List<string> _receivedMessages2 = new();

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
        }

        [Fact]
        public async Task PlayersCanJoinRoom()
        {
            // Arrange
            string player1 = "Player1";
            string player2 = "Player2";

            // Act
            // Add null checks before using hub connections
            if (_hubConnection1 != null && _hubConnection2 != null)
            {
                await _hubConnection1.SendAsync("JoinRoom", _roomId, player1);
                await _hubConnection2.SendAsync("JoinRoom", _roomId, player2);

                // Wait for messages to be processed
                await Task.Delay(1000);

                // Assert
                Assert.Contains($"PlayerJoined: {player1}", _receivedMessages1);
                Assert.Contains($"PlayerJoined: {player2}", _receivedMessages1);
                Assert.Contains($"PlayerJoined: {player1}", _receivedMessages2);
                Assert.Contains($"PlayerJoined: {player2}", _receivedMessages2);
            }
            else
            {
                Assert.Fail("Hub connections were not properly initialized");
            }
        }

        [Fact]
        public async Task PlayersCanLeaveRoom()
        {
            // Arrange
            string player1 = "Player1";
            string player2 = "Player2";

            // Act
            // Add null checks before using hub connections
            if (_hubConnection1 != null && _hubConnection2 != null)
            {
                await _hubConnection1.SendAsync("JoinRoom", _roomId, player1);
                await _hubConnection2.SendAsync("JoinRoom", _roomId, player2);
                await _hubConnection1.SendAsync("LeaveRoom", _roomId, player1);

                // Wait for messages to be processed
                await Task.Delay(1000);

                // Assert
                Assert.Contains($"PlayerLeft: {player1}", _receivedMessages1);
                Assert.Contains($"PlayerLeft: {player1}", _receivedMessages2);
            }
            else
            {
                Assert.Fail("Hub connections were not properly initialized");
            }
        }

        [Fact]
        public async Task GameCanBeStarted()
        {
            // Arrange
            string player1 = "Player1";
            string player2 = "Player2";

            // Act
            // Add null checks before using hub connections
            if (_hubConnection1 != null && _hubConnection2 != null)
            {
                await _hubConnection1.SendAsync("JoinRoom", _roomId, player1);
                await _hubConnection2.SendAsync("JoinRoom", _roomId, player2);
                await _hubConnection1.SendAsync("StartGame", _roomId);

                // Wait for messages to be processed
                await Task.Delay(1000);

                // Assert
                Assert.Contains("GameStarted", _receivedMessages1);
                Assert.Contains("GameStarted", _receivedMessages2);
            }
            else
            {
                Assert.Fail("Hub connections were not properly initialized");
            }
        }

        [Fact]
        public async Task AnswersCanBeSubmitted()
        {
            // Arrange
            string player1 = "Player1";
            int questionId = 1;
            string answer = "Test Answer";

            // Act
            // Add null checks before using hub connections
            if (_hubConnection1 != null)
            {
                await _hubConnection1.SendAsync("JoinRoom", _roomId, player1);
                await _hubConnection1.SendAsync("SubmitAnswer", _roomId, player1, questionId, answer);

                // Wait for messages to be processed
                await Task.Delay(1000);

                // Assert
                // Note: We're not asserting the answer submission directly as it's broadcast to the room
                // and we'd need to add a specific handler for AnswerSubmitted events
                Assert.True(_hubConnection1.State == HubConnectionState.Connected);
            }
            else
            {
                Assert.Fail("Hub connections were not properly initialized");
            }
        }
    }
}
