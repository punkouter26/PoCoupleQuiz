using Microsoft.AspNetCore.SignalR;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;

namespace PoCoupleQuiz.Server.Hubs;

/// <summary>
/// SignalR hub handling the full remote-multiplayer lifecycle:
/// lobby creation/joining -> game start -> per-round answers -> results -> game over.
/// Game state is authoritative in <see cref="IGameSessionManager"/>.
/// Clients are pure views driven by hub events.
/// </summary>
public class GameHub : Hub
{
    private readonly ILogger<GameHub> _logger;
    private readonly IGameSessionManager _sessions;
    private readonly IQuestionService _questionService;

    public GameHub(
        ILogger<GameHub> logger,
        IGameSessionManager sessions,
        IQuestionService questionService)
    {
        _logger = logger;
        _sessions = sessions;
        _questionService = questionService;
    }

    // === LOBBY PHASE =======================================================

    /// <summary>Host creates a new lobby. Returns game code to caller.</summary>
    public async Task CreateLobby(string playerName, string difficulty)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            await Clients.Caller.SendAsync("LobbyError", "Player name cannot be empty.");
            return;
        }

        var diffLevel = Enum.TryParse<DifficultyLevel>(difficulty, true, out var d) ? d : DifficultyLevel.Medium;
        var session = _sessions.CreateLobby(Context.ConnectionId, playerName.Trim(), diffLevel);

        await Groups.AddToGroupAsync(Context.ConnectionId, session.GameCode);
        _logger.LogInformation("Lobby {Code} created by {Host}", session.GameCode, playerName);

        await Clients.Caller.SendAsync("LobbyCreated", new
        {
            GameCode = session.GameCode,
            IsHost = true,
            Players = session.Players.Select(p => p.Name).ToList(),
            HostName = session.HostName,
            Difficulty = session.Difficulty.ToString()
        });
    }

    /// <summary>Non-host player joins an existing lobby using the game code.</summary>
    public async Task JoinLobby(string gameCode, string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName) || string.IsNullOrWhiteSpace(gameCode))
        {
            await Clients.Caller.SendAsync("LobbyError", "Game code and player name are required.");
            return;
        }

        var session = _sessions.JoinLobby(gameCode.Trim().ToUpperInvariant(), Context.ConnectionId, playerName.Trim());

        if (session == null)
        {
            await Clients.Caller.SendAsync("LobbyError",
                "Could not join. The game code may be invalid, the game may have already started, or that name is already taken.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, session.GameCode);
        _logger.LogInformation("Player {Name} joined lobby {Code}", playerName, session.GameCode);

        // Tell the new joiner their context
        await Clients.Caller.SendAsync("LobbyJoined", new
        {
            GameCode = session.GameCode,
            IsHost = false,
            Players = session.Players.Select(p => p.Name).ToList(),
            HostName = session.HostName,
            Difficulty = session.Difficulty.ToString()
        });

        // Notify everyone else about updated player list
        await Clients.OthersInGroup(session.GameCode).SendAsync("LobbyUpdated", new
        {
            GameCode = session.GameCode,
            Players = session.Players.Select(p => p.Name).ToList(),
            HostName = session.HostName,
            Difficulty = session.Difficulty.ToString()
        });
    }

    // === SINGLE-INSTANCE CONVENIENCE ======================================

    /// <summary>
    /// Auto-joins the only waiting lobby, or creates one if none exists.
    /// Eliminates the need for players to share/enter a game code.
    /// </summary>
    public async Task JoinOrCreate(string playerName, string difficulty)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            await Clients.Caller.SendAsync("LobbyError", "Player name cannot be empty.");
            return;
        }

        var existing = _sessions.GetWaitingLobby();

        if (existing == null)
        {
            // No lobby — become host
            var diffLevel = Enum.TryParse<DifficultyLevel>(difficulty, true, out var d) ? d : DifficultyLevel.Medium;
            var session = _sessions.CreateLobby(Context.ConnectionId, playerName.Trim(), diffLevel);
            await Groups.AddToGroupAsync(Context.ConnectionId, session.GameCode);
            _logger.LogInformation("JoinOrCreate: lobby {Code} created by {Host}", session.GameCode, playerName);
            await Clients.Caller.SendAsync("LobbyCreated", new
            {
                GameCode = session.GameCode,
                IsHost = true,
                Players = session.Players.Select(p => p.Name).ToList(),
                HostName = session.HostName,
                Difficulty = session.Difficulty.ToString()
            });
        }
        else
        {
            // Lobby exists — join it
            var session = _sessions.JoinLobby(existing.GameCode, Context.ConnectionId, playerName.Trim());
            if (session == null)
            {
                await Clients.Caller.SendAsync("LobbyError", "Could not join. That name may already be taken.");
                return;
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, session.GameCode);
            _logger.LogInformation("JoinOrCreate: {Name} joined lobby {Code}", playerName, session.GameCode);
            await Clients.Caller.SendAsync("LobbyJoined", new
            {
                GameCode = session.GameCode,
                IsHost = false,
                Players = session.Players.Select(p => p.Name).ToList(),
                HostName = session.HostName,
                Difficulty = session.Difficulty.ToString()
            });
            await Clients.OthersInGroup(session.GameCode).SendAsync("LobbyUpdated", new
            {
                GameCode = session.GameCode,
                Players = session.Players.Select(p => p.Name).ToList(),
                HostName = session.HostName,
                Difficulty = session.Difficulty.ToString()
            });
        }
    }

    // === GAME START ========================================================

    /// <summary>
    /// Host starts the game. Requires at least 2 players.
    /// Generates the first question and broadcasts GameStarted.
    /// </summary>
    public async Task StartGame(string gameCode)
    {
        var session = _sessions.GetSession(gameCode);

        if (session == null)
        {
            await Clients.Caller.SendAsync("LobbyError", "Session not found.");
            return;
        }

        if (session.HostConnectionId != Context.ConnectionId)
        {
            await Clients.Caller.SendAsync("LobbyError", "Only the host can start the game.");
            return;
        }

        if (session.Players.Count < 2)
        {
            await Clients.Caller.SendAsync("LobbyError", "At least 2 players are required to start.");
            return;
        }

        if (session.State != SessionState.Waiting)
        {
            await Clients.Caller.SendAsync("LobbyError", "Game has already started.");
            return;
        }

        Question firstQuestion;
        try
        {
            firstQuestion = await _questionService.GenerateQuestionAsync(session.Difficulty.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate first question for {Code}", gameCode);
            await Clients.Caller.SendAsync("LobbyError", "Failed to generate question. Please try again.");
            return;
        }

        var game = _sessions.StartGame(gameCode, firstQuestion.Text, firstQuestion.Category.ToString());
        _logger.LogInformation("Game started for lobby {Code}", gameCode);

        await Clients.Group(gameCode).SendAsync("GameStarted", new
        {
            GameCode = gameCode,
            KingPlayerName = game.KingPlayer?.Name,
            Players = game.Players.Select(p => new { p.Name, p.IsKingPlayer, p.Score }).ToList(),
            QuestionText = firstQuestion.Text,
            QuestionCategory = firstQuestion.Category.ToString(),
            RoundIndex = 0,
            MaxRounds = game.MaxRounds,
            Difficulty = game.Difficulty.ToString()
        });
    }

    // === ANSWER SUBMISSION =================================================

    /// <summary>
    /// Any player submits their answer for the current round.
    /// When ALL players have answered, evaluates with AI and broadcasts RoundResult.
    /// </summary>
    public async Task SubmitAnswer(string gameCode, string answer, int roundIndex)
    {
        var session = _sessions.GetSession(gameCode);
        if (session?.ActiveGame == null)
        {
            await Clients.Caller.SendAsync("GameError", "Session not found or game not started.");
            return;
        }

        var lobbyPlayer = session.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
        if (lobbyPlayer == null)
        {
            await Clients.Caller.SendAsync("GameError", "You are not part of this game.");
            return;
        }

        var playerName = lobbyPlayer.Name;
        var trimmed = answer.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            await Clients.Caller.SendAsync("GameError", "Answer cannot be empty.");
            return;
        }

        _logger.LogInformation("Player {Name} submitted answer in {Code} round {Round}", playerName, gameCode, roundIndex);

        // Acknowledge to the others (without revealing the answer)
        await Clients.OthersInGroup(gameCode).SendAsync("AnswerRecorded", new
        {
            PlayerName = playerName,
            RoundIndex = roundIndex
        });

        bool allAnswered = _sessions.RecordAnswer(gameCode, playerName, trimmed);

        if (allAnswered)
        {
            await EvaluateAndBroadcastRoundResult(gameCode, session, roundIndex);
        }
    }

    // === ROUND ADVANCEMENT =================================================

    /// <summary>
    /// Host advances to the next round or ends the game.
    /// Generates a new question and broadcasts RoundStarted or GameOver.
    /// </summary>
    public async Task RequestNextRound(string gameCode)
    {
        var session = _sessions.GetSession(gameCode);
        if (session?.ActiveGame == null)
        {
            await Clients.Caller.SendAsync("GameError", "Session not found.");
            return;
        }

        if (session.HostConnectionId != Context.ConnectionId)
        {
            await Clients.Caller.SendAsync("GameError", "Only the host can advance the round.");
            return;
        }

        var game = _sessions.AdvanceRound(gameCode);

        if (game.IsGameOver)
        {
            _sessions.FinishGame(gameCode);
            await Clients.Group(gameCode).SendAsync("GameOver", new
            {
                GameCode = gameCode,
                FinalScores = game.GetScoreboard(),
                Players = game.Players.Select(p => new { p.Name, p.Score }).ToList()
            });
            return;
        }

        Question nextQuestion;
        try
        {
            nextQuestion = await _questionService.GenerateQuestionAsync(game.Difficulty.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate question for round {Round} in {Code}", game.CurrentRound, gameCode);
            await Clients.Caller.SendAsync("GameError", "Failed to generate next question. Please try again.");
            game.CurrentRound--;
            return;
        }

        var gq = new GameQuestion
        {
            Question = nextQuestion.Text,
            Category = nextQuestion.Category
        };
        game.Questions.Add(gq);
        session.CurrentQuestion = gq;

        await Clients.Group(gameCode).SendAsync("RoundStarted", new
        {
            GameCode = gameCode,
            RoundIndex = game.CurrentRound,
            MaxRounds = game.MaxRounds,
            KingPlayerName = game.KingPlayer?.Name,
            Players = game.Players.Select(p => new { p.Name, p.IsKingPlayer, p.Score }).ToList(),
            QuestionText = nextQuestion.Text,
            QuestionCategory = nextQuestion.Category.ToString()
        });
    }

    // === DISCONNECTION =====================================================

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
            _logger.LogWarning(exception, "Client {Id} disconnected with error", Context.ConnectionId);
        else
            _logger.LogDebug("Client {Id} disconnected cleanly", Context.ConnectionId);

        var session = _sessions.RemovePlayer(Context.ConnectionId, out bool sessionEmpty);

        if (session == null || sessionEmpty)
        {
            await base.OnDisconnectedAsync(exception);
            return;
        }

        var gameCode = session.GameCode;

        await Clients.Group(gameCode).SendAsync("PlayerDisconnected", new
        {
            GameCode = gameCode,
            Players = session.Players.Select(p => p.Name).ToList(),
            HostName = session.HostName
        });

        // If the host disconnected, promote the next player
        if (!session.Players.Any(p => p.ConnectionId == session.HostConnectionId))
        {
            var newHost = _sessions.PromoteNextHost(gameCode);
            if (newHost != null)
            {
                await Clients.Group(gameCode).SendAsync("HostChanged", new
                {
                    GameCode = gameCode,
                    NewHostName = newHost
                });
            }
        }

        // During a game, give the disconnected player 30s to reconnect before skipping them
        if (session.State == SessionState.InProgress && session.ActiveGame != null)
        {
            _ = HandleDisconnectGrace(session, gameCode);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task HandleDisconnectGrace(GameSession session, string gameCode)
    {
        session.DisconnectGraceCts?.Cancel();
        var cts = new CancellationTokenSource();
        session.DisconnectGraceCts = cts;

        try { await Task.Delay(TimeSpan.FromSeconds(30), cts.Token); }
        catch (TaskCanceledException) { return; }

        // Auto-fill blank for any player still missing their answer
        foreach (var p in session.Players.ToList())
        {
            if (!session.RoundAnswers.TryGetValue(p.Name, out var ans) || ans == null)
                _sessions.RecordAnswer(gameCode, p.Name, "(no answer)");
        }

        if (session.RoundAnswers.Values.All(v => v != null) && session.ActiveGame != null)
        {
            var roundIndex = session.ActiveGame.CurrentRound;
            await EvaluateAndBroadcastRoundResult(gameCode, session, roundIndex);
        }
    }

    // === INTERNAL HELPERS ==================================================

    private async Task EvaluateAndBroadcastRoundResult(string gameCode, GameSession session, int roundIndex)
    {
        var game = session.ActiveGame!;
        var question = session.CurrentQuestion!;
        var kingAnswer = question.KingPlayerAnswer;
        var matchedPlayers = new List<string>();
        var roundPoints = new Dictionary<string, int>();

        foreach (var (playerName, answer) in question.PlayerAnswers)
        {
            if (string.IsNullOrWhiteSpace(answer) || answer == "(no answer)") continue;
            try
            {
                var isMatch = await _questionService.CheckAnswerSimilarityAsync(kingAnswer, answer);
                if (isMatch)
                {
                    matchedPlayers.Add(playerName);
                    roundPoints[playerName] = GameEngine.PointsPerCorrectAnswer;
                    question.MarkPlayerAsMatched(playerName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Similarity check failed for player {Name} in {Code}", playerName, gameCode);
            }
        }

        _sessions.ApplyRoundScores(gameCode, roundPoints);

        await Clients.Group(gameCode).SendAsync("RoundResult", new
        {
            GameCode = gameCode,
            RoundIndex = roundIndex,
            KingAnswer = kingAnswer,
            MatchedPlayers = matchedPlayers,
            Scores = game.GetScoreboard(),
            Players = game.Players.Select(p => new { p.Name, p.Score }).ToList()
        });
    }

    // === LEGACY PASS-THROUGH ===============================================

    /// <summary>Legacy join by raw game ID (kept for backward compatibility).</summary>
    public async Task JoinGame(string gameId, string playerName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        _logger.LogInformation("Player {PlayerName} joined game {GameId}", playerName, gameId);
        await Clients.OthersInGroup(gameId).SendAsync("PlayerJoined", new
        {
            PlayerName = playerName,
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    /// <summary>Legacy leave by raw game ID.</summary>
    public async Task LeaveGame(string gameId, string playerName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
        await Clients.OthersInGroup(gameId).SendAsync("PlayerLeft", new
        {
            PlayerName = playerName,
            Timestamp = DateTimeOffset.UtcNow
        });
    }
}
