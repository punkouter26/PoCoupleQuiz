using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

/// <summary>
/// Service for generating quiz questions and evaluating answer similarity using Azure AI Foundry.
/// </summary>
public interface IQuestionService
{
    /// <summary>
    /// Generates a quiz question using Azure AI Foundry (Azure OpenAI).
    /// </summary>
    /// <param name="difficulty">Optional difficulty level: easy, medium, or hard.</param>
    /// <returns>A generated Question object.</returns>
    Task<Question> GenerateQuestionAsync(string? difficulty = null);

    /// <summary>
    /// Checks if two answers are semantically similar using Azure AI Foundry.
    /// </summary>
    /// <param name="answer1">The King player's answer.</param>
    /// <param name="answer2">The guessing player's answer.</param>
    /// <returns>True if answers are considered a match.</returns>
    Task<bool> CheckAnswerSimilarityAsync(string answer1, string answer2);
}
