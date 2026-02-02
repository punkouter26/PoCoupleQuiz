using Microsoft.AspNetCore.SignalR;
using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Server.Hubs;

/// <summary>
/// SignalR hub for real-time game state synchronization.
/// Enables live updates for multiplayer gameplay without polling.
/// </summary>
public class GameHub : Hub
{
    private readonly ILogger<GameHub> _logger;

    public GameHub(ILogger<GameHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Join a game room for real-time updates.
    /// </summary>
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

    /// <summary>
    /// Leave a game room.
    /// </summary>
    public async Task LeaveGame(string gameId, string playerName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
        
        _logger.LogInformation("Player {PlayerName} left game {GameId}", playerName, gameId);
        
        await Clients.OthersInGroup(gameId).SendAsync("PlayerLeft", new
        {
            PlayerName = playerName,
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Notify all players in game that King has answered.
    /// </summary>
    public async Task KingAnswered(string gameId, int roundIndex)
    {
        _logger.LogInformation("King answered in game {GameId}, round {RoundIndex}", gameId, roundIndex);
        
        await Clients.OthersInGroup(gameId).SendAsync("KingAnswered", new
        {
            RoundIndex = roundIndex,
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Notify all players that a guesser has submitted their answer.
    /// </summary>
    public async Task PlayerAnswered(string gameId, string playerName, int roundIndex)
    {
        _logger.LogInformation("Player {PlayerName} answered in game {GameId}, round {RoundIndex}", 
            playerName, gameId, roundIndex);
        
        await Clients.OthersInGroup(gameId).SendAsync("PlayerAnswered", new
        {
            PlayerName = playerName,
            RoundIndex = roundIndex,
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Broadcast round results to all players.
    /// </summary>
    public async Task RoundCompleted(string gameId, int roundIndex, Dictionary<string, int> scores)
    {
        _logger.LogInformation("Round {RoundIndex} completed in game {GameId}", roundIndex, gameId);
        
        await Clients.Group(gameId).SendAsync("RoundCompleted", new
        {
            RoundIndex = roundIndex,
            Scores = scores,
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Broadcast game completion to all players.
    /// </summary>
    public async Task GameCompleted(string gameId, Dictionary<string, int> finalScores)
    {
        _logger.LogInformation("Game {GameId} completed", gameId);
        
        await Clients.Group(gameId).SendAsync("GameCompleted", new
        {
            FinalScores = finalScores,
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Handle disconnection cleanup.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogWarning(exception, "Client disconnected with error: {ConnectionId}", Context.ConnectionId);
        }
        else
        {
            _logger.LogDebug("Client disconnected: {ConnectionId}", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
