using System.Collections.Concurrent;
using PoCoupleQuiz.Core.Models;
using Microsoft.Extensions.Logging;

namespace PoCoupleQuiz.Core.Services;

/// <summary>
/// Represents a player waiting in a lobby or actively in a game.
/// </summary>
public record LobbyPlayer(string Name, string ConnectionId);

/// <summary>
/// Lifecycle state of a lobby/game session.
/// </summary>
public enum SessionState
{
    Waiting,
    InProgress,
    Finished
}

/// <summary>
/// Combined lobby + active game session keyed by a short game code (e.g. "XK9Z4T").
/// Thread-safe for concurrent SignalR connections.
/// </summary>
public class GameSession
{
    public string GameCode { get; init; } = string.Empty;
    public string HostConnectionId { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
    public SessionState State { get; set; } = SessionState.Waiting;
    public List<LobbyPlayer> Players { get; set; } = new();

    // --- Active game state (populated after StartGame) ---
    public Game? ActiveGame { get; set; }
    public GameQuestion? CurrentQuestion { get; set; }

    /// <summary>Per-round: playerName → submitted answer (null = not yet answered).</summary>
    public Dictionary<string, string?> RoundAnswers { get; set; } = new();

    /// <summary>Cancellation to skip a disconnected player after the grace period.</summary>
    public CancellationTokenSource? DisconnectGraceCts { get; set; }

    public string? HostName => Players.FirstOrDefault(p => p.ConnectionId == HostConnectionId)?.Name;
}

/// <summary>
/// Server-side singleton that owns all <see cref="GameSession"/> objects.
/// Game state is authoritative here; clients are pure views driven by SignalR events.
/// </summary>
public interface IGameSessionManager
{
    /// <summary>Creates a new lobby, returns the 6-char game code.</summary>
    GameSession CreateLobby(string hostConnectionId, string hostName, DifficultyLevel difficulty);

    /// <summary>Adds a player to an existing lobby. Returns null if the code is invalid or game already started.</summary>
    GameSession? JoinLobby(string gameCode, string connectionId, string playerName);

    /// <summary>Removes a player from their session. Returns the session so the caller can broadcast.</summary>
    GameSession? RemovePlayer(string connectionId, out bool sessionEmpty);

    /// <summary>Returns the session associated with the given connectionId, or null.</summary>
    GameSession? GetSessionByConnection(string connectionId);

    /// <summary>Returns the session for a game code, or null.</summary>
    GameSession? GetSession(string gameCode);

    /// <summary>Validates that a game code exists and the session is in the Waiting state.</summary>
    bool LobbyExists(string gameCode);

    /// <summary>Initialises the server-side <see cref="Game"/> and transitions to InProgress.</summary>
    Game StartGame(string gameCode, string questionText, string category);

    /// <summary>Records one playerʼs answer. Returns true when ALL active players have answered.</summary>
    bool RecordAnswer(string gameCode, string playerName, string answer);

    /// <summary>Advances to the next round: resets answers, rotates king, bumps CurrentRound. Returns the updated game.</summary>
    Game AdvanceRound(string gameCode);

    /// <summary>Updates scores after evaluation. Keyed by playerName → additional points this round.</summary>
    void ApplyRoundScores(string gameCode, Dictionary<string, int> pointsEarned);

    /// <summary>Marks the session as Finished and removes it after a delay.</summary>
    void FinishGame(string gameCode);

    /// <summary>Transfers host role to the next available player. Returns the new hostʼs name.</summary>
    string? PromoteNextHost(string gameCode);

    /// <summary>Returns the first lobby in Waiting state, or null if none exist.</summary>
    GameSession? GetWaitingLobby();
}

/// <inheritdoc />
public class GameSessionManager : IGameSessionManager
{
    // Primary index: gameCode → session
    private readonly ConcurrentDictionary<string, GameSession> _sessions = new(StringComparer.OrdinalIgnoreCase);
    // Secondary index: connectionId → gameCode  (so we can look up by connection on disconnect)
    private readonly ConcurrentDictionary<string, string> _connToCode = new();

    private readonly ILogger<GameSessionManager> _logger;

    public GameSessionManager(ILogger<GameSessionManager> logger)
    {
        _logger = logger;
    }

    // ── Factory ─────────────────────────────────────────────────────────────

    public GameSession CreateLobby(string hostConnectionId, string hostName, DifficultyLevel difficulty)
    {
        var code = GenerateCode();
        var session = new GameSession
        {
            GameCode = code,
            HostConnectionId = hostConnectionId,
            Difficulty = difficulty,
            State = SessionState.Waiting,
            Players = new List<LobbyPlayer> { new LobbyPlayer(hostName, hostConnectionId) }
        };

        _sessions[code] = session;
        _connToCode[hostConnectionId] = code;

        _logger.LogInformation("Lobby created: {Code} by {Host}", code, hostName);
        return session;
    }

    public GameSession? JoinLobby(string gameCode, string connectionId, string playerName)
    {
        if (!_sessions.TryGetValue(gameCode, out var session))
        {
            _logger.LogWarning("JoinLobby: code {Code} not found", gameCode);
            return null;
        }

        if (session.State != SessionState.Waiting)
        {
            _logger.LogWarning("JoinLobby: game {Code} already started", gameCode);
            return null;
        }

        // Prevent duplicate names
        if (session.Players.Any(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("JoinLobby: name {Name} already taken in {Code}", playerName, gameCode);
            return null;
        }

        lock (session)
        {
            session.Players.Add(new LobbyPlayer(playerName, connectionId));
        }
        _connToCode[connectionId] = gameCode;

        _logger.LogInformation("Player {Name} joined lobby {Code}", playerName, gameCode);
        return session;
    }

    public GameSession? GetWaitingLobby()
        => _sessions.Values.FirstOrDefault(s => s.State == SessionState.Waiting);

    public GameSession? RemovePlayer(string connectionId, out bool sessionEmpty)
    {
        sessionEmpty = false;

        if (!_connToCode.TryRemove(connectionId, out var code))
            return null;

        if (!_sessions.TryGetValue(code, out var session))
            return null;

        lock (session)
        {
            session.Players.RemoveAll(p => p.ConnectionId == connectionId);
        }

        sessionEmpty = session.Players.Count == 0;
        if (sessionEmpty)
        {
            _sessions.TryRemove(code, out _);
            _logger.LogInformation("Session {Code} removed — no players left", code);
        }

        return session;
    }

    public GameSession? GetSessionByConnection(string connectionId)
    {
        if (!_connToCode.TryGetValue(connectionId, out var code)) return null;
        _sessions.TryGetValue(code, out var session);
        return session;
    }

    public GameSession? GetSession(string gameCode)
    {
        _sessions.TryGetValue(gameCode, out var session);
        return session;
    }

    public bool LobbyExists(string gameCode) =>
        _sessions.TryGetValue(gameCode, out var s) && s.State == SessionState.Waiting;

    // ── Game lifecycle ───────────────────────────────────────────────────────

    public Game StartGame(string gameCode, string questionText, string category)
    {
        var session = GetSessionOrThrow(gameCode);

        var game = new Game { Difficulty = session.Difficulty };

        // First player in the lobby list is the initial King
        foreach (var (lobbyPlayer, index) in session.Players.Select((p, i) => (p, i)))
        {
            game.AddPlayer(new Player
            {
                Name = lobbyPlayer.Name,
                IsKingPlayer = index == 0
            });
        }

        // Prepare first question
        var firstQuestion = new GameQuestion
        {
            Question = questionText,
            Category = Enum.TryParse<QuestionCategory>(category, true, out var cat)
                ? cat
                : QuestionCategory.Relationships
        };
        game.Questions.Add(firstQuestion);
        game.GameSessionId = gameCode; // Use game code as session ID for easy reference

        session.ActiveGame = game;
        session.CurrentQuestion = firstQuestion;
        session.RoundAnswers = session.Players.ToDictionary(p => p.Name, _ => (string?)null);
        session.State = SessionState.InProgress;

        _logger.LogInformation("Game started for lobby {Code} with {Count} players", gameCode, game.Players.Count);
        return game;
    }

    public bool RecordAnswer(string gameCode, string playerName, string answer)
    {
        var session = GetSessionOrThrow(gameCode);

        lock (session)
        {
            session.RoundAnswers[playerName] = answer;

            // Record in active game question too
            var q = session.CurrentQuestion;
            if (q != null)
            {
                var isKing = session.ActiveGame?.KingPlayer?.Name.Equals(playerName, StringComparison.Ordinal) ?? false;
                if (isKing)
                    q.KingPlayerAnswer = answer;
                else
                    q.RecordPlayerAnswer(playerName, answer);
            }

            // All players answered?
            return session.RoundAnswers.Values.All(v => v != null);
        }
    }

    public Game AdvanceRound(string gameCode)
    {
        var session = GetSessionOrThrow(gameCode);
        var game = session.ActiveGame ?? throw new InvalidOperationException("Game not started");

        game.CurrentRound++;
        // King never rotates — same player stays king for the entire game.

        // Reset answers for new round
        session.RoundAnswers = session.Players.ToDictionary(p => p.Name, _ => (string?)null);

        _logger.LogInformation("Round advanced to {Round} in game {Code}", game.CurrentRound, gameCode);
        return game;
    }

    public void ApplyRoundScores(string gameCode, Dictionary<string, int> pointsEarned)
    {
        var session = GetSessionOrThrow(gameCode);
        if (session.ActiveGame == null) return;

        foreach (var (playerName, points) in pointsEarned)
        {
            var player = session.ActiveGame.Players.FirstOrDefault(
                p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));
            if (player != null)
                player.Score += points;
        }
    }

    public void FinishGame(string gameCode)
    {
        if (_sessions.TryGetValue(gameCode, out var session))
        {
            session.State = SessionState.Finished;
            _logger.LogInformation("Game {Code} finished", gameCode);

            // Clean up after 10 minutes so late-arrivals can still read final state
            _ = Task.Delay(TimeSpan.FromMinutes(10))
                    .ContinueWith(t => _sessions.TryRemove(gameCode, out GameSession? removed));
        }
    }

    public string? PromoteNextHost(string gameCode)
    {
        if (!_sessions.TryGetValue(gameCode, out var session)) return null;

        lock (session)
        {
            if (!session.Players.Any()) return null;

            var newHost = session.Players.First();
            session.HostConnectionId = newHost.ConnectionId;
            _logger.LogInformation("Host promoted to {Name} in {Code}", newHost.Name, gameCode);
            return newHost.Name;
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private GameSession GetSessionOrThrow(string gameCode)
    {
        if (!_sessions.TryGetValue(gameCode, out var session))
            throw new InvalidOperationException($"Session {gameCode} not found");
        return session;
    }

    /// <summary>Generates a 6-char alphanumeric code formatted as XXX-YYY.</summary>
    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // omit confusable chars
        var random = new Random();
        var part1 = new string(Enumerable.Range(0, 3).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        var part2 = new string(Enumerable.Range(0, 3).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        return $"{part1}-{part2}";
    }
}
