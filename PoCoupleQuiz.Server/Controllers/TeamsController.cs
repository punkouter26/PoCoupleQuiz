using Microsoft.AspNetCore.Mvc;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;

namespace PoCoupleQuiz.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Team>>> GetAllTeams()
    {
        var teams = await _teamService.GetAllTeamsAsync();
        return Ok(teams);
    }

    [HttpGet("{teamName}")]
    public async Task<ActionResult<Team>> GetTeam(string teamName)
    {
        var team = await _teamService.GetTeamAsync(teamName);
        if (team == null)
        {
            return NotFound();
        }
        return Ok(team);
    }

    [HttpPost]
    public async Task<ActionResult> SaveTeam([FromBody] Team team)
    {
        await _teamService.SaveTeamAsync(team);
        return Ok();
    }

    [HttpPut("{teamName}/stats")]
    public async Task<ActionResult> UpdateTeamStats(string teamName, [FromBody] UpdateStatsRequest request)
    {
        await _teamService.UpdateTeamStatsAsync(teamName, request.GameMode, request.Score);
        return Ok();
    }
}

public class UpdateStatsRequest
{
    public GameMode GameMode { get; set; }
    public int Score { get; set; }
}
