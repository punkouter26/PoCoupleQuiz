using Microsoft.AspNetCore.Mvc;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;

namespace PoCoupleQuiz.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameHistoryController : ControllerBase
{
    private readonly IGameHistoryService _gameHistoryService;
    private readonly ILogger<GameHistoryController> _logger;

    public GameHistoryController(IGameHistoryService gameHistoryService, ILogger<GameHistoryController> logger)
    {
        _gameHistoryService = gameHistoryService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SaveGameHistory([FromBody] GameHistory history)
    {
        try
        {
            if (history == null)
            {
                _logger.LogWarning("Received null GameHistory object");
                return BadRequest("GameHistory object is required");
            }

            if (string.IsNullOrWhiteSpace(history.Team1Name) && string.IsNullOrWhiteSpace(history.Team2Name))
            {
                _logger.LogWarning("GameHistory missing team names");
                return BadRequest("At least one team name is required");
            }

            if (history.TotalQuestions < 0)
            {
                _logger.LogWarning("GameHistory has invalid TotalQuestions: {TotalQuestions}", history.TotalQuestions);
                return BadRequest("TotalQuestions must be non-negative");
            }

            // Log telemetry with structured properties
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Team1Name"] = history.Team1Name,
                ["Team2Name"] = history.Team2Name,
                ["TotalQuestions"] = history.TotalQuestions,
                ["Team1Score"] = history.Team1Score,
                ["Team2Score"] = history.Team2Score,
                ["GameMode"] = history.GameMode.ToString()
            }))
            {
                _logger.LogInformation(
                    "Saving game history: {Team1} ({Team1Score}) vs {Team2} ({Team2Score}), Mode: {GameMode}, Questions: {TotalQuestions}",
                    history.Team1Name,
                    history.Team1Score,
                    history.Team2Name,
                    history.Team2Score,
                    history.GameMode,
                    history.TotalQuestions);
            }

            await _gameHistoryService.SaveGameHistoryAsync(history);

            _logger.LogInformation("Successfully saved game history");
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving game history");
            return StatusCode(500, "An error occurred while saving game history");
        }
    }

    [HttpGet("team/{teamName}")]
    public async Task<ActionResult<IEnumerable<GameHistory>>> GetTeamHistory(string teamName)
    {
        var history = await _gameHistoryService.GetTeamHistoryAsync(teamName);
        return Ok(history);
    }

    [HttpGet("categoryStats/{teamName}")]
    public async Task<ActionResult<Dictionary<QuestionCategory, int>>> GetTeamCategoryStats(string teamName)
    {
        var stats = await _gameHistoryService.GetTeamCategoryStatsAsync(teamName);
        return Ok(stats);
    }

    [HttpGet("topMatchedAnswers/{teamName}/{count}")]
    public async Task<ActionResult<List<string>>> GetTopMatchedAnswers(string teamName, int count)
    {
        var answers = await _gameHistoryService.GetTopMatchedAnswersAsync(teamName, count);
        return Ok(answers);
    }

    [HttpGet("averageResponseTime/{teamName}")]
    public async Task<ActionResult<double>> GetAverageResponseTime(string teamName)
    {
        var avgTime = await _gameHistoryService.GetAverageResponseTimeAsync(teamName);
        return Ok(avgTime);
    }
}
