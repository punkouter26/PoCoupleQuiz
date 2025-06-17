using Microsoft.AspNetCore.Mvc;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;

namespace PoCoupleQuiz.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameHistoryController : ControllerBase
{
    private readonly IGameHistoryService _gameHistoryService;

    public GameHistoryController(IGameHistoryService gameHistoryService)
    {
        _gameHistoryService = gameHistoryService;
    }

    [HttpPost]
    public async Task<IActionResult> SaveGameHistory([FromBody] GameHistory history)
    {
        await _gameHistoryService.SaveGameHistoryAsync(history);
        return Ok();
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
