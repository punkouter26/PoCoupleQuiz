using PoCoupleQuiz.Core.Services;

namespace PoCoupleQuiz.Tests;

public class MockQuestionService : IQuestionService
{
    private int _questionCounter = 0;
    private readonly string[] _predefinedQuestions = new[]
    {
        "What is your partner's favorite color?",
        "What is your partner's favorite food?",
        "Where was your first date?",
        "What is your partner's dream vacation destination?"
    };

    private readonly Dictionary<string, string> _predefinedAnswers = new()
    {
        { "What is your partner's favorite color?", "Blue" },
        { "What is your partner's favorite food?", "Pizza" },
        { "Where was your first date?", "Restaurant" },
        { "What is your partner's dream vacation destination?", "Paris" }
    };

    public Task<string> GenerateQuestionAsync()
    {
        var question = _predefinedQuestions[_questionCounter % _predefinedQuestions.Length];
        _questionCounter++;
        return Task.FromResult(question);
    }

    public Task<bool> CheckAnswerSimilarityAsync(string answer1, string answer2)
    {
        // Simple string comparison rules for testing
        if (string.IsNullOrWhiteSpace(answer1) || string.IsNullOrWhiteSpace(answer2))
            return Task.FromResult(false);

        // Exact match
        if (answer1.Equals(answer2, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(true);

        // Known similar answers
        var normalizedAnswer1 = answer1.Trim().ToLower();
        var normalizedAnswer2 = answer2.Trim().ToLower();

        // Handle date format variations
        if (normalizedAnswer1.Contains("january 1st") && normalizedAnswer2.Contains("1st of january"))
            return Task.FromResult(true);

        // Handle pizza variations
        if (normalizedAnswer1.Contains("pizza") && normalizedAnswer2.Contains("pizza"))
            return Task.FromResult(true);

        return Task.FromResult(false);
    }

    public Task<string> GenerateAnswerAsync(string question)
    {
        // Return predefined answer if available, otherwise return a generic answer
        if (_predefinedAnswers.TryGetValue(question, out var answer))
        {
            return Task.FromResult(answer);
        }
        return Task.FromResult("I don't know");
    }
}