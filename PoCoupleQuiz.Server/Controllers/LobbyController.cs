using Microsoft.AspNetCore.Mvc;
using PoCoupleQuiz.Core.Services;

namespace PoCoupleQuiz.Server.Controllers;

/// <summary>
/// REST endpoints for lobby management (validation, status).
/// The primary lobby lifecycle is driven by SignalR (GameHub);
/// these endpoints exist for lightweight polling and page-refresh recovery.
/// </summary>
[ApiController]
[Route("api/lobby")]
public class LobbyController : ControllerBase
{
    private readonly IGameSessionManager _sessions;
    private readonly ILogger<LobbyController> _logger;

    public LobbyController(IGameSessionManager sessions, ILogger<LobbyController> logger)
    {
        _sessions = sessions;
        _logger = logger;
    }

    /// <summary>
    /// Returns whether a lobby with the given code currently exists and is open for joining.
    /// Used by the client to validate a code before opening a SignalR connection.
    /// GET /api/lobby/{gameCode}/exists
    /// </summary>
    [HttpGet("{gameCode}/exists")]
    public IActionResult LobbyExists(string gameCode)
    {
        if (string.IsNullOrWhiteSpace(gameCode))
            return BadRequest(new { exists = false, error = "Game code is required." });

        var exists = _sessions.LobbyExists(gameCode.Trim().ToUpperInvariant());
        return Ok(new { exists });
    }

    /// <summary>
    /// Returns the current state of a session (players, host, difficulty, state) without requiring SignalR.
    /// Useful for page-refresh recovery so the client knows whether to redirect home.
    /// GET /api/lobby/{gameCode}/status
    /// </summary>
    [HttpGet("{gameCode}/status")]
    public IActionResult GetStatus(string gameCode)
    {
        if (string.IsNullOrWhiteSpace(gameCode))
            return BadRequest(new { error = "Game code is required." });

        var session = _sessions.GetSession(gameCode.Trim().ToUpperInvariant());
        if (session == null)
            return NotFound(new { error = "Session not found." });

        return Ok(new
        {
            GameCode = session.GameCode,
            State = session.State.ToString(),
            HostName = session.HostName,
            Players = session.Players.Select(p => p.Name).ToList(),
            Difficulty = session.Difficulty.ToString(),
            PlayerCount = session.Players.Count
        });
    }
}
