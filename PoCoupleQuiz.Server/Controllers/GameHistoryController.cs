using Microsoft.AspNetCore.Mvc;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Core.Validators;
using PoCoupleQuiz.Server.Filters;

namespace PoCoupleQuiz.Server.Controllers;

[ApiController]
[Route("api/game-history")]
public class GameHistoryController : ControllerBase
{
    private readonly IGameHistoryService _gameHistoryService;
    private readonly ILogger<GameHistoryController> _logger;
    private readonly IValidator<string> _teamNameValidator;

    public GameHistoryController(
        IGameHistoryService gameHistoryService,
        ILogger<GameHistoryController> logger,
        IValidator<string> teamNameValidator)
    {
        _gameHistoryService = gameHistoryService;
        _logger = logger;
        _teamNameValidator = teamNameValidator;
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

            // Validate team names
            if (!string.IsNullOrWhiteSpace(history.Team1Name))
            {
                var team1Validation = _teamNameValidator.Validate(history.Team1Name);
                if (!team1Validation.IsValid)
                {
                    return BadRequest($"Team1Name: {team1Validation.ErrorMessage}");
                }
            }

            if (!string.IsNullOrWhiteSpace(history.Team2Name))
            {
                var team2Validation = _teamNameValidator.Validate(history.Team2Name);
                if (!team2Validation.IsValid)
                {
                    return BadRequest($"Team2Name: {team2Validation.ErrorMessage}");
                }
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
                ["Team2Score"] = history.Team2Score
            }))
            {
                _logger.LogInformation(
                    "Saving game history: {Team1} ({Team1Score}) vs {Team2} ({Team2Score}), Questions: {TotalQuestions}",
                    history.Team1Name,
                    history.Team1Score,
                    history.Team2Name,
                    history.Team2Score,
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

    [HttpGet("teams/{teamName}")]
    [ValidateTeamName]
    public async Task<ActionResult<IEnumerable<GameHistory>>> GetTeamHistory(string teamName)
    {
        var history = await _gameHistoryService.GetTeamHistoryAsync(teamName);
        return Ok(history);
    }

    [HttpGet("teams/{teamName}/category-stats")]
    [ValidateTeamName]
    public async Task<ActionResult<Dictionary<QuestionCategory, int>>> GetTeamCategoryStats(string teamName)
    {
        var stats = await _gameHistoryService.GetTeamCategoryStatsAsync(teamName);
        return Ok(stats);
    }

    [HttpGet("teams/{teamName}/top-matched-answers/{count}")]
    [ValidateTeamName]
    public async Task<ActionResult<List<string>>> GetTopMatchedAnswers(string teamName, int count)
    {
        var answers = await _gameHistoryService.GetTopMatchedAnswersAsync(teamName, count);
        return Ok(answers);
    }

    [HttpGet("teams/{teamName}/average-response-time")]
    [ValidateTeamName]
    public async Task<ActionResult<double>> GetAverageResponseTime(string teamName)
    {
        var avgTime = await _gameHistoryService.GetAverageResponseTimeAsync(teamName);
        return Ok(avgTime);
    }
}
