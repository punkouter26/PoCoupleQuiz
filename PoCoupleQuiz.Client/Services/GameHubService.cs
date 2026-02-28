using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace PoCoupleQuiz.Client.Services;

// ── DTOs (received from server broadcasts) ─────────────────────────────────

public record LobbyInfo(
    string GameCode,
    bool IsHost,
    List<string> Players,
    string HostName,
    string Difficulty);

public record GameStartedInfo(
    string GameCode,
    string KingPlayerName,
    List<PlayerInfo> Players,
    string QuestionText,
    string QuestionCategory,
    int RoundIndex,
    int MaxRounds,
    string Difficulty);

public record RoundStartedInfo(
    string GameCode,
    int RoundIndex,
    int MaxRounds,
    string KingPlayerName,
    List<PlayerInfo> Players,
    string QuestionText,
    string QuestionCategory);

public record RoundResultInfo(
    string GameCode,
    int RoundIndex,
    string KingAnswer,
    List<string> MatchedPlayers,
    Dictionary<string, int> Scores,
    List<PlayerInfo> Players);

public record GameOverInfo(
    string GameCode,
    Dictionary<string, int> FinalScores,
    List<PlayerInfo> Players);

public record PlayerInfo(string Name, bool IsKingPlayer, int Score);

public record LobbyUpdateInfo(
    string GameCode,
    List<string> Players,
    string HostName,
    string? Difficulty = null);

// ── Interface ───────────────────────────────────────────────────────────────

/// <summary>
/// Client-side SignalR service for the full remote-multiplayer flow.
/// </summary>
public interface IGameHubService : IAsyncDisposable
{
    bool IsConnected { get; }

    /// <summary>Name of the local player — set when CreateLobby/JoinLobby is invoked.</summary>
    string MyPlayerName { get; }

    /// <summary>The game code the local player is participating in.</summary>
    string MyGameCode { get; }

    /// <summary>True if the local player is the lobby host on this device.</summary>
    bool IsHost { get; }

    /// <summary>The last lobby state received (created or joined). Used to seed Lobby page on navigation.</summary>
    LobbyInfo? CurrentLobby { get; }

    /// <summary>The last GameStarted payload received. Used to seed Game page after navigation.</summary>
    GameStartedInfo? CurrentGame { get; }

    Task ConnectAsync();

    // ── Lobby invocations ──────────────────────────────────────────────────
    /// <summary>Auto-joins the existing waiting lobby or creates one (single-instance mode).</summary>
    Task JoinOrCreateAsync(string playerName, string difficulty);
    Task CreateLobbyAsync(string playerName, string difficulty);
    Task JoinLobbyAsync(string gameCode, string playerName);

    // ── Game invocations ───────────────────────────────────────────────────
    Task StartGameAsync(string gameCode);
    Task SubmitAnswerAsync(string gameCode, string answer, int roundIndex);
    Task RequestNextRoundAsync(string gameCode);

    // ── Lobby events ───────────────────────────────────────────────────────
    event Action<LobbyInfo>? OnLobbyCreated;
    event Action<LobbyInfo>? OnLobbyJoined;
    event Action<LobbyUpdateInfo>? OnLobbyUpdated;
    event Action<string>? OnLobbyError;

    // ── Game events ────────────────────────────────────────────────────────
    event Action<GameStartedInfo>? OnGameStarted;
    event Action<RoundStartedInfo>? OnRoundStarted;
    event Action<string, int>? OnAnswerRecorded;   // (playerName, roundIndex)
    event Action<RoundResultInfo>? OnRoundResult;
    event Action<GameOverInfo>? OnGameOver;
    event Action<string>? OnGameError;

    // ── Connection/presence events ─────────────────────────────────────────
    event Action<LobbyUpdateInfo>? OnPlayerDisconnected;
    event Action<string, string>? OnHostChanged;  // (gameCode, newHostName)

    // ── Legacy events (kept for backward compat) ───────────────────────────
    event Action<string>? OnPlayerJoined;
    event Action<string>? OnPlayerLeft;
}

// ── Implementation ──────────────────────────────────────────────────────────

/// <summary>
/// Implementation of <see cref="IGameHubService"/> wrapping a SignalR HubConnection.
/// </summary>
public class GameHubService : IGameHubService
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<GameHubService> _logger;

    public bool IsConnected => _hubConnection.State == HubConnectionState.Connected;

    /// <summary>Name of the local player on this device.</summary>
    public string MyPlayerName { get; private set; } = "";

    /// <summary>Game code the local player has joined or created.</summary>
    public string MyGameCode { get; private set; } = "";

    /// <summary>Whether local player is the lobby host.</summary>
    public bool IsHost { get; private set; }

    /// <summary>Last lobby state received — lets Lobby page seed itself after navigation.</summary>
    public LobbyInfo? CurrentLobby { get; private set; }

    /// <summary>Last GameStarted payload — lets Game page seed itself after navigation.</summary>
    public GameStartedInfo? CurrentGame { get; private set; }

    // Lobby events
    public event Action<LobbyInfo>? OnLobbyCreated;
    public event Action<LobbyInfo>? OnLobbyJoined;
    public event Action<LobbyUpdateInfo>? OnLobbyUpdated;
    public event Action<string>? OnLobbyError;

    // Game events
    public event Action<GameStartedInfo>? OnGameStarted;
    public event Action<RoundStartedInfo>? OnRoundStarted;
    public event Action<string, int>? OnAnswerRecorded;
    public event Action<RoundResultInfo>? OnRoundResult;
    public event Action<GameOverInfo>? OnGameOver;
    public event Action<string>? OnGameError;

    // Presence events
    public event Action<LobbyUpdateInfo>? OnPlayerDisconnected;
    public event Action<string, string>? OnHostChanged;

    // Legacy events
    public event Action<string>? OnPlayerJoined;
    public event Action<string>? OnPlayerLeft;

    public GameHubService(IConfiguration configuration, NavigationManager navigationManager, ILogger<GameHubService> logger)
    {
        _logger = logger;

        var baseUrl = configuration["ApiBaseUrl"] ?? "";
        // NavigationManager.BaseUri ends with '/', so use it to form an absolute URL.
        // Relative URLs like "/hubs/game" resolve to file:// in WASM and are blocked.
        var hubUrl = string.IsNullOrEmpty(baseUrl)
            ? $"{navigationManager.BaseUri}hubs/game"
            : $"{baseUrl}/hubs/game";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        RegisterHandlers();
    }

    private void RegisterHandlers()
    {
        // ── Lobby ──────────────────────────────────────────────────────────

        _hubConnection.On<JsonElement>("LobbyCreated", data =>
        {
            try
            {
                var info = ParseLobbyInfo(data, isHost: true);
                MyGameCode = info.GameCode;
                IsHost = true;
                CurrentLobby = info;
                _logger.LogInformation("Lobby created: {Code}", info.GameCode);
                OnLobbyCreated?.Invoke(info);
            }
            catch (Exception ex) { _logger.LogError(ex, "Error parsing LobbyCreated"); }
        });

        _hubConnection.On<JsonElement>("LobbyJoined", data =>
        {
            try
            {
                var info = ParseLobbyInfo(data, isHost: false);
                MyGameCode = info.GameCode;
                IsHost = false;
                CurrentLobby = info;
                _logger.LogInformation("Lobby joined: {Code}", info.GameCode);
                OnLobbyJoined?.Invoke(info);
            }
            catch (Exception ex) { _logger.LogError(ex, "Error parsing LobbyJoined"); }
        });

        _hubConnection.On<JsonElement>("LobbyUpdated", data =>
        {
            try
            {
                var update = ParseLobbyUpdate(data);
                OnLobbyUpdated?.Invoke(update);
            }
            catch (Exception ex) { _logger.LogError(ex, "Error parsing LobbyUpdated"); }
        });

        _hubConnection.On<string>("LobbyError", msg =>
        {
            _logger.LogWarning("LobbyError: {Msg}", msg);
            OnLobbyError?.Invoke(msg);
        });

        // ── Game ───────────────────────────────────────────────────────────

        _hubConnection.On<JsonElement>("GameStarted", data =>
        {
            try
            {
                var info = new GameStartedInfo(
                    GameCode: data.GetProperty("gameCode").GetString() ?? "",
                    KingPlayerName: data.GetProperty("kingPlayerName").GetString() ?? "",
                    Players: ParsePlayerList(data.GetProperty("players")),
                    QuestionText: data.GetProperty("questionText").GetString() ?? "",
                    QuestionCategory: data.GetProperty("questionCategory").GetString() ?? "",
                    RoundIndex: data.GetProperty("roundIndex").GetInt32(),
                    MaxRounds: data.GetProperty("maxRounds").GetInt32(),
                    Difficulty: data.GetProperty("difficulty").GetString() ?? "Medium"
                );
                _logger.LogInformation("Game started: {Code}", info.GameCode);
                CurrentGame = info;
                OnGameStarted?.Invoke(info);
            }
            catch (Exception ex) { _logger.LogError(ex, "Error parsing GameStarted"); }
        });

        _hubConnection.On<JsonElement>("RoundStarted", data =>
        {
            try
            {
                var info = new RoundStartedInfo(
                    GameCode: data.GetProperty("gameCode").GetString() ?? "",
                    RoundIndex: data.GetProperty("roundIndex").GetInt32(),
                    MaxRounds: data.GetProperty("maxRounds").GetInt32(),
                    KingPlayerName: data.GetProperty("kingPlayerName").GetString() ?? "",
                    Players: ParsePlayerList(data.GetProperty("players")),
                    QuestionText: data.GetProperty("questionText").GetString() ?? "",
                    QuestionCategory: data.GetProperty("questionCategory").GetString() ?? ""
                );
                _logger.LogInformation("Round {Round} started in {Code}", info.RoundIndex, info.GameCode);
                OnRoundStarted?.Invoke(info);
            }
            catch (Exception ex) { _logger.LogError(ex, "Error parsing RoundStarted"); }
        });

        _hubConnection.On<JsonElement>("AnswerRecorded", data =>
        {
            try
            {
                var playerName = data.GetProperty("playerName").GetString() ?? "";
                var roundIndex = data.GetProperty("roundIndex").GetInt32();
                OnAnswerRecorded?.Invoke(playerName, roundIndex);
            }
            catch (Exception ex) { _logger.LogError(ex, "Error parsing AnswerRecorded"); }
        });

        _hubConnection.On<JsonElement>("RoundResult", data =>
        {
            try
            {
                var scores = new Dictionary<string, int>();
                foreach (var prop in data.GetProperty("scores").EnumerateObject())
                    scores[prop.Name] = prop.Value.GetInt32();

                var matched = new List<string>();
                foreach (var item in data.GetProperty("matchedPlayers").EnumerateArray())
                    matched.Add(item.GetString() ?? "");

                var info = new RoundResultInfo(
                    GameCode: data.GetProperty("gameCode").GetString() ?? "",
                    RoundIndex: data.GetProperty("roundIndex").GetInt32(),
                    KingAnswer: data.GetProperty("kingAnswer").GetString() ?? "",
                    MatchedPlayers: matched,
                    Scores: scores,
                    Players: ParsePlayerList(data.GetProperty("players"))
                );
                _logger.LogInformation("Round result for {Code}", info.GameCode);
                OnRoundResult?.Invoke(info);
            }
            catch (Exception ex) { _logger.LogError(ex, "Error parsing RoundResult"); }
        });

        _hubConnection.On<JsonElement>("GameOver", data =>
        {
            try
            {
                var finalScores = new Dictionary<string, int>();
                foreach (var prop in data.GetProperty("finalScores").EnumerateObject())
                    finalScores[prop.Name] = prop.Value.GetInt32();

                var info = new GameOverInfo(
                    GameCode: data.GetProperty("gameCode").GetString() ?? "",
                    FinalScores: finalScores,
                    Players: ParsePlayerList(data.GetProperty("players"))
                );
                _logger.LogInformation("Game over for {Code}", info.GameCode);
                OnGameOver?.Invoke(info);
            }
            catch (Exception ex) { _logger.LogError(ex, "Error parsing GameOver"); }
        });

        _hubConnection.On<string>("GameError", msg =>
        {
            _logger.LogWarning("GameError: {Msg}", msg);
            OnGameError?.Invoke(msg);
        });

        // ── Presence ──────────────────────────────────────────────────────

        _hubConnection.On<JsonElement>("PlayerDisconnected", data =>
        {
            try { OnPlayerDisconnected?.Invoke(ParseLobbyUpdate(data)); }
            catch (Exception ex) { _logger.LogError(ex, "Error parsing PlayerDisconnected"); }
        });

        _hubConnection.On<JsonElement>("HostChanged", data =>
        {
            try
            {
                var code = data.GetProperty("gameCode").GetString() ?? "";
                var host = data.GetProperty("newHostName").GetString() ?? "";
                // Update local host flag if we became the new host
                if (host.Equals(MyPlayerName, StringComparison.OrdinalIgnoreCase))
                    IsHost = true;
                OnHostChanged?.Invoke(code, host);
            }
            catch (Exception ex) { _logger.LogError(ex, "Error parsing HostChanged"); }
        });

        // ── Legacy ────────────────────────────────────────────────────────

        _hubConnection.On<JsonElement>("PlayerJoined", data =>
        {
            try { OnPlayerJoined?.Invoke(data.GetProperty("playerName").GetString() ?? ""); }
            catch { /* ignore */ }
        });

        _hubConnection.On<JsonElement>("PlayerLeft", data =>
        {
            try { OnPlayerLeft?.Invoke(data.GetProperty("playerName").GetString() ?? ""); }
            catch { /* ignore */ }
        });

        // ── Connection lifecycle ───────────────────────────────────────────

        _hubConnection.Reconnecting += error =>
        {
            _logger.LogWarning(error, "SignalR reconnecting...");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += _ =>
        {
            _logger.LogInformation("SignalR reconnected");
            return Task.CompletedTask;
        };

        _hubConnection.Closed += error =>
        {
            _logger.LogWarning(error, "SignalR connection closed");
            return Task.CompletedTask;
        };
    }

    // ── Public API ──────────────────────────────────────────────────────────

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

    public async Task JoinOrCreateAsync(string playerName, string difficulty)
    {
        MyPlayerName = playerName;
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("JoinOrCreate", playerName, difficulty);
    }

    public async Task CreateLobbyAsync(string playerName, string difficulty)
    {
        MyPlayerName = playerName;
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("CreateLobby", playerName, difficulty);
    }

    public async Task JoinLobbyAsync(string gameCode, string playerName)
    {
        MyPlayerName = playerName;
        MyGameCode = gameCode.ToUpperInvariant();
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("JoinLobby", gameCode, playerName);
    }

    public async Task StartGameAsync(string gameCode)
    {
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("StartGame", gameCode);
    }

    public async Task SubmitAnswerAsync(string gameCode, string answer, int roundIndex)
    {
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("SubmitAnswer", gameCode, answer, roundIndex);
    }

    public async Task RequestNextRoundAsync(string gameCode)
    {
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("RequestNextRound", gameCode);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private async Task EnsureConnectedAsync()
    {
        if (!IsConnected)
            await ConnectAsync();
    }

    private static LobbyInfo ParseLobbyInfo(JsonElement data, bool isHost)
    {
        var players = new List<string>();
        foreach (var item in data.GetProperty("players").EnumerateArray())
            players.Add(item.GetString() ?? "");

        return new LobbyInfo(
            GameCode: data.GetProperty("gameCode").GetString() ?? "",
            IsHost: data.TryGetProperty("isHost", out var h) ? h.GetBoolean() : isHost,
            Players: players,
            HostName: data.GetProperty("hostName").GetString() ?? "",
            Difficulty: data.GetProperty("difficulty").GetString() ?? "Medium"
        );
    }

    private static LobbyUpdateInfo ParseLobbyUpdate(JsonElement data)
    {
        var players = new List<string>();
        foreach (var item in data.GetProperty("players").EnumerateArray())
            players.Add(item.GetString() ?? "");

        return new LobbyUpdateInfo(
            GameCode: data.GetProperty("gameCode").GetString() ?? "",
            Players: players,
            HostName: data.GetProperty("hostName").GetString() ?? "",
            Difficulty: data.TryGetProperty("difficulty", out var d) ? d.GetString() : null
        );
    }

    private static List<PlayerInfo> ParsePlayerList(JsonElement arr)
    {
        var list = new List<PlayerInfo>();
        foreach (var item in arr.EnumerateArray())
        {
            list.Add(new PlayerInfo(
                Name: item.GetProperty("name").GetString() ?? "",
                IsKingPlayer: item.TryGetProperty("isKingPlayer", out var k) && k.GetBoolean(),
                Score: item.TryGetProperty("score", out var s) ? s.GetInt32() : 0
            ));
        }
        return list;
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection.State != HubConnectionState.Disconnected)
            await _hubConnection.StopAsync();
        await _hubConnection.DisposeAsync();
    }
}
