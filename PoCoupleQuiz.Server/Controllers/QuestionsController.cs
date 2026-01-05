using Microsoft.AspNetCore.Mvc;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;

namespace PoCoupleQuiz.Server.Controllers;

/// <summary>
/// Controller for question generation and answer similarity checking using Azure AI Foundry.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly ILogger<QuestionsController> _logger;

    public QuestionsController(IQuestionService questionService, ILogger<QuestionsController> logger)
    {
        _questionService = questionService;
        _logger = logger;
    }

    /// <summary>
    /// Generates a quiz question using Azure AI Foundry.
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<Question>> GenerateQuestion([FromBody] GenerateQuestionRequest? request)
    {
        try
        {
            _logger.LogInformation("Generating question with difficulty: {Difficulty}", request?.Difficulty ?? "default");
            var question = await _questionService.GenerateQuestionAsync(request?.Difficulty);
            return Ok(question);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating question: {Message}", ex.Message);
            return StatusCode(500, new { error = "Failed to generate question", details = ex.Message });
        }
    }

    /// <summary>
    /// Checks semantic similarity between two answers using Azure AI Foundry.
    /// </summary>
    [HttpPost("check-similarity")]
    public async Task<ActionResult<bool>> CheckSimilarity([FromBody] CheckSimilarityRequest request)
    {
        try
        {
            _logger.LogInformation("Checking similarity between answers");
            var result = await _questionService.CheckAnswerSimilarityAsync(request.Answer1, request.Answer2);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking answer similarity");
            return StatusCode(500, new { error = "Failed to check similarity" });
        }
    }
}

public record GenerateQuestionRequest(string? Difficulty);
public record CheckSimilarityRequest(string Answer1, string Answer2);
