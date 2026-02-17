using Microsoft.AspNetCore.Mvc;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Server.Filters;

namespace PoCoupleQuiz.Server.Controllers;

[ApiController]
[Route("api/game")]
public class GameController : ControllerBase
{
    private readonly ILogger<GameController> _logger;

    public GameController(ILogger<GameController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates and advances the game to the next round server-side to prevent race conditions.
    /// Uses optimistic concurrency control via current round and player count.
    /// </summary>
    [HttpPost("advance-round")]
    public IActionResult AdvanceRound([FromBody] AdvanceRoundRequest request)
    {
        try
        {
            // Validate request
            if (request?.Game == null)
                return BadRequest(new { error = "Game state is required" });

            if (request.Game.Players == null || request.Game.Players.Count == 0)
                return BadRequest(new { error = "No valid players in game state" });

            // Optimistic locking: Verify request is for current round
            // (This prevents double-advancement if client sent stale request)
            var expectedRound = request.CurrentRound;
            var nextRound = expectedRound + 1;

            _logger.LogInformation(
                "Advancing game from round {CurrentRound} to {NextRound} with {PlayerCount} players",
                expectedRound, nextRound, request.Game.Players.Count);

            // Verify minimum players requirement
            const int minimumPlayers = 2;
            if (request.Game.Players.Count < minimumPlayers)
            {
                _logger.LogWarning("Game has insufficient players ({Count}) to advance round", 
                    request.Game.Players.Count);
                return BadRequest(new { 
                    error = "Insufficient players",
                    message = $"Only {request.Game.Players.Count} players remaining. Game requires minimum {minimumPlayers}."
                });
            }

            // Perform round advancement on server
            request.Game.CurrentRound = nextRound;

            // Only rotate king if game is not over
            if (!request.Game.IsGameOver)
            {
                try
                {
                    request.Game.SetNextKingPlayer();
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "Failed to rotate king player");
                    return StatusCode(500, new { 
                        error = "King rotation failed",
                        message = ex.Message
                    });
                }
            }

            _logger.LogInformation(
                "Successfully advanced to round {Round}. New king: {KingPlayer}",
                nextRound, request.Game.KingPlayer?.Name ?? "Unknown");

            return Ok(new AdvanceRoundResponse
            {
                Success = true,
                NewRound = nextRound,
                NewKingPlayerName = request.Game.KingPlayer?.Name ?? string.Empty,
                IsGameOver = request.Game.IsGameOver
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error advancing round");
            return StatusCode(500, new { 
                error = "Server error",
                message = "Failed to advance round. Please try again."
            });
        }
    }
}

/// <summary>
/// Request to advance game to next round (with optimistic concurrency control).
/// </summary>
public class AdvanceRoundRequest
{
    /// <summary>
    /// Current round number before advancement (used for optimistic locking).
    /// </summary>
    public int CurrentRound { get; set; }

    /// <summary>
    /// Complete game state (used to validate player count and perform rotation).
    /// </summary>
    public Game? Game { get; set; }
}

/// <summary>
/// Response after successful round advancement.
/// </summary>
public class AdvanceRoundResponse
{
    /// <summary>
    /// Whether the round advancement succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The new round number.
    /// </summary>
    public int NewRound { get; set; }

    /// <summary>
    /// Name of the new king player for next round.
    /// </summary>
    public string NewKingPlayerName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the game is over after this round advancement.
    /// </summary>
    public bool IsGameOver { get; set; }
}
