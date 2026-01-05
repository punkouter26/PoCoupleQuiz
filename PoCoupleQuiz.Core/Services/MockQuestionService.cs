using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

/// <summary>
/// Mock implementation of IQuestionService for development/testing when Azure AI Foundry is not available.
/// </summary>
public class MockQuestionService : IQuestionService
{
    private int _questionCounter = 0;

    private readonly string[] _easyQuestions =
    [
        "What is your partner's favorite color?",
        "Do they prefer coffee or tea?",
        "What is their favorite season?"
    ];

    private readonly string[] _mediumQuestions =
    [
        "What is your partner's favorite food?",
        "Where was your first date?",
        "What is your partner's dream vacation destination?",
        "What time do they usually go to bed?"
    ];

    private readonly string[] _hardQuestions =
    [
        "What was their most defining life experience?",
        "How do they approach conflict resolution?",
        "What was their childhood ambition?"
    ];

    public Task<Question> GenerateQuestionAsync(string? difficulty = null)
    {
        string[] questionSet = difficulty?.ToLower() switch
        {
            "easy" => _easyQuestions,
            "hard" => _hardQuestions,
            _ => _mediumQuestions
        };

        var questionText = questionSet[_questionCounter % questionSet.Length];
        _questionCounter++;
        var question = new Question { Text = questionText, Category = QuestionCategory.Preferences };
        return Task.FromResult(question);
    }

    public Task<bool> CheckAnswerSimilarityAsync(string answer1, string answer2)
    {
        if (string.IsNullOrWhiteSpace(answer1) || string.IsNullOrWhiteSpace(answer2))
            return Task.FromResult(false);

        if (answer1.Equals(answer2, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(true);

        var normalizedAnswer1 = answer1.Trim().ToLower();
        var normalizedAnswer2 = answer2.Trim().ToLower();

        // Simple similarity checks for testing
        if (normalizedAnswer1.Contains(normalizedAnswer2) || normalizedAnswer2.Contains(normalizedAnswer1))
            return Task.FromResult(true);

        return Task.FromResult(false);
    }
}
