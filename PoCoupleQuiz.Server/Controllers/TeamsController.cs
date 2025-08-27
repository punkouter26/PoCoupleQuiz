using Microsoft.AspNetCore.Mvc;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;
using System.ComponentModel.DataAnnotations;

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
        if (string.IsNullOrWhiteSpace(teamName))
        {
            return BadRequest("Team name cannot be empty.");
        }

        if (teamName.Length > 100)
        {
            return BadRequest("Team name cannot exceed 100 characters.");
        }

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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(team.Name))
        {
            return BadRequest("Team name is required.");
        }

        if (team.Name.Length > 100)
        {
            return BadRequest("Team name cannot exceed 100 characters.");
        }

        if (team.TotalQuestionsAnswered < 0 || team.CorrectAnswers < 0)
        {
            return BadRequest("Statistics cannot be negative.");
        }

        await _teamService.SaveTeamAsync(team);
        return Ok();
    }

    [HttpPut("{teamName}/stats")]
    public async Task<ActionResult> UpdateTeamStats(string teamName, [FromBody] UpdateStatsRequest request)
    {
        if (string.IsNullOrWhiteSpace(teamName))
        {
            return BadRequest("Team name cannot be empty.");
        }

        if (teamName.Length > 100)
        {
            return BadRequest("Team name cannot exceed 100 characters.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (request.Score < 0)
        {
            return BadRequest("Score cannot be negative.");
        }

        // Validate enum value
        if (!Enum.IsDefined(typeof(GameMode), request.GameMode))
        {
            return BadRequest("Invalid game mode.");
        }

        await _teamService.UpdateTeamStatsAsync(teamName, request.GameMode, request.Score);
        return Ok();
    }
}

public class UpdateStatsRequest
{
    [Required]
    public GameMode GameMode { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Score must be non-negative")]
    public int Score { get; set; }
}
