using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace PoCoupleQuiz.Client.Services;

/// <summary>
/// Client-side SignalR service for real-time game updates.
/// </summary>
public interface IGameHubService : IAsyncDisposable
{
    /// <summary>
    /// Indicates if connected to the hub.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Connect to the game hub.
    /// </summary>
    Task ConnectAsync();

    /// <summary>
    /// Join a specific game room.
    /// </summary>
    Task JoinGameAsync(string gameId, string playerName);

    /// <summary>
    /// Leave the current game room.
    /// </summary>
    Task LeaveGameAsync(string gameId, string playerName);

    /// <summary>
    /// Notify that the King has answered.
    /// </summary>
    Task NotifyKingAnsweredAsync(string gameId, int roundIndex);

    /// <summary>
    /// Notify that a player has answered.
    /// </summary>
    Task NotifyPlayerAnsweredAsync(string gameId, string playerName, int roundIndex);

    /// <summary>
    /// Notify that a round is complete.
    /// </summary>
    Task NotifyRoundCompletedAsync(string gameId, int roundIndex, Dictionary<string, int> scores);

    /// <summary>
    /// Notify that the game is complete.
    /// </summary>
    Task NotifyGameCompletedAsync(string gameId, Dictionary<string, int> finalScores);

    /// <summary>
    /// Event raised when another player joins.
    /// </summary>
    event Action<string>? OnPlayerJoined;

    /// <summary>
    /// Event raised when another player leaves.
    /// </summary>
    event Action<string>? OnPlayerLeft;

    /// <summary>
    /// Event raised when King answers.
    /// </summary>
    event Action<int>? OnKingAnswered;

    /// <summary>
    /// Event raised when a player answers.
    /// </summary>
    event Action<string, int>? OnPlayerAnswered;

    /// <summary>
    /// Event raised when round completes.
    /// </summary>
    event Action<int, Dictionary<string, int>>? OnRoundCompleted;

    /// <summary>
    /// Event raised when game completes.
    /// </summary>
    event Action<Dictionary<string, int>>? OnGameCompleted;
}

/// <summary>
/// Implementation of the SignalR game hub client.
/// </summary>
public class GameHubService : IGameHubService
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<GameHubService> _logger;

    public bool IsConnected => _hubConnection.State == HubConnectionState.Connected;

    public event Action<string>? OnPlayerJoined;
    public event Action<string>? OnPlayerLeft;
    public event Action<int>? OnKingAnswered;
    public event Action<string, int>? OnPlayerAnswered;
    public event Action<int, Dictionary<string, int>>? OnRoundCompleted;
    public event Action<Dictionary<string, int>>? OnGameCompleted;

    public GameHubService(IConfiguration configuration, ILogger<GameHubService> logger)
    {
        _logger = logger;

        var baseUrl = configuration["ApiBaseUrl"] ?? "";
        var hubUrl = string.IsNullOrEmpty(baseUrl) ? "/hubs/game" : $"{baseUrl}/hubs/game";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        RegisterHandlers();
    }

    private void RegisterHandlers()
    {
        _hubConnection.On<dynamic>("PlayerJoined", (data) =>
        {
            var playerName = (string)data.PlayerName;
            _logger.LogInformation("Player joined: {PlayerName}", playerName);
            OnPlayerJoined?.Invoke(playerName);
        });

        _hubConnection.On<dynamic>("PlayerLeft", (data) =>
        {
            var playerName = (string)data.PlayerName;
            _logger.LogInformation("Player left: {PlayerName}", playerName);
            OnPlayerLeft?.Invoke(playerName);
        });

        _hubConnection.On<dynamic>("KingAnswered", (data) =>
        {
            var roundIndex = (int)data.RoundIndex;
            _logger.LogInformation("King answered for round {RoundIndex}", roundIndex);
            OnKingAnswered?.Invoke(roundIndex);
        });

        _hubConnection.On<dynamic>("PlayerAnswered", (data) =>
        {
            var playerName = (string)data.PlayerName;
            var roundIndex = (int)data.RoundIndex;
            _logger.LogInformation("Player {PlayerName} answered for round {RoundIndex}", playerName, roundIndex);
            OnPlayerAnswered?.Invoke(playerName, roundIndex);
        });

        _hubConnection.On<dynamic>("RoundCompleted", (data) =>
        {
            var roundIndex = (int)data.RoundIndex;
            var scores = ((IDictionary<string, object>)data.Scores)
                .ToDictionary(kvp => kvp.Key, kvp => Convert.ToInt32(kvp.Value));
            _logger.LogInformation("Round {RoundIndex} completed", roundIndex);
            OnRoundCompleted?.Invoke(roundIndex, scores);
        });

        _hubConnection.On<dynamic>("GameCompleted", (data) =>
        {
            var finalScores = ((IDictionary<string, object>)data.FinalScores)
                .ToDictionary(kvp => kvp.Key, kvp => Convert.ToInt32(kvp.Value));
            _logger.LogInformation("Game completed");
            OnGameCompleted?.Invoke(finalScores);
        });

        _hubConnection.Reconnecting += error =>
        {
            _logger.LogWarning(error, "SignalR connection lost, attempting to reconnect...");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            _logger.LogInformation("SignalR reconnected with connection ID: {ConnectionId}", connectionId);
            return Task.CompletedTask;
        };

        _hubConnection.Closed += error =>
        {
            _logger.LogWarning(error, "SignalR connection closed");
            return Task.CompletedTask;
        };
    }

    public async Task ConnectAsync()
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            try
            {
                await _hubConnection.StartAsync();
                _logger.LogInformation("Connected to GameHub");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to GameHub");
                throw;
            }
        }
    }

    public async Task JoinGameAsync(string gameId, string playerName)
    {
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("JoinGame", gameId, playerName);
        _logger.LogInformation("Joined game {GameId} as {PlayerName}", gameId, playerName);
    }

    public async Task LeaveGameAsync(string gameId, string playerName)
    {
        if (IsConnected)
        {
            await _hubConnection.InvokeAsync("LeaveGame", gameId, playerName);
            _logger.LogInformation("Left game {GameId}", gameId);
        }
    }

    public async Task NotifyKingAnsweredAsync(string gameId, int roundIndex)
    {
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("KingAnswered", gameId, roundIndex);
    }

    public async Task NotifyPlayerAnsweredAsync(string gameId, string playerName, int roundIndex)
    {
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("PlayerAnswered", gameId, playerName, roundIndex);
    }

    public async Task NotifyRoundCompletedAsync(string gameId, int roundIndex, Dictionary<string, int> scores)
    {
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("RoundCompleted", gameId, roundIndex, scores);
    }

    public async Task NotifyGameCompletedAsync(string gameId, Dictionary<string, int> finalScores)
    {
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("GameCompleted", gameId, finalScores);
    }

    private async Task EnsureConnectedAsync()
    {
        if (!IsConnected)
        {
            await ConnectAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection.State != HubConnectionState.Disconnected)
        {
            await _hubConnection.StopAsync();
        }
        await _hubConnection.DisposeAsync();
    }
}
