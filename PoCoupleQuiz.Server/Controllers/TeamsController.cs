using Microsoft.AspNetCore.Mvc;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Core.Validators;
using PoCoupleQuiz.Server.Filters;
using System.ComponentModel.DataAnnotations;

namespace PoCoupleQuiz.Server.Controllers;

[ApiController]
[Route("api/teams")]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;
    private readonly ILogger<TeamsController> _logger;
    private readonly IValidator<string> _teamNameValidator;

    public TeamsController(
        ITeamService teamService,
        ILogger<TeamsController> logger,
        IValidator<string> teamNameValidator)
    {
        _teamService = teamService;
        _logger = logger;
        _teamNameValidator = teamNameValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Team>>> GetAllTeams()
    {
        _logger.LogInformation("Retrieving all teams");
        var teams = await _teamService.GetAllTeamsAsync();

        using (_logger.BeginScope(new Dictionary<string, object?> { ["TeamCount"] = teams.Count() }))
        {
            _logger.LogInformation("Retrieved {TeamCount} teams", teams.Count());
        }

        return Ok(teams);
    }

    [HttpGet("{teamName}")]
    [ValidateTeamName]
    public async Task<ActionResult<Team>> GetTeam(string teamName)
    {
        var validationResult = _teamNameValidator.Validate(teamName);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ErrorMessage);
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

        var validationResult = _teamNameValidator.Validate(team.Name);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ErrorMessage);
        }

        if (team.TotalQuestionsAnswered < 0 || team.CorrectAnswers < 0)
        {
            return BadRequest("Statistics cannot be negative.");
        }

        await _teamService.SaveTeamAsync(team);
        return Ok();
    }

    [HttpPut("{teamName}/stats")]
    [ValidateTeamName]
    public async Task<ActionResult> UpdateTeamStats(string teamName, [FromBody] UpdateStatsRequest request)
    {
        var validationResult = _teamNameValidator.Validate(teamName);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ErrorMessage);
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

        await _teamService.UpdateTeamStatsAsync(teamName, request.GameMode, request.Score, request.QuestionsAnswered, request.CorrectAnswers);
        return Ok();
    }
}

public class UpdateStatsRequest
{
    [Required]
    public GameMode GameMode { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Score must be non-negative")]
    public int Score { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Questions answered must be non-negative")]
    public int QuestionsAnswered { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Correct answers must be non-negative")]
    public int CorrectAnswers { get; set; }
}
