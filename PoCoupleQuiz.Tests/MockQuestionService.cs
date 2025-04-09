using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Core.Models; // Added using

namespace PoCoupleQuiz.Tests;

public class MockQuestionService : IQuestionService
{
    private int _questionCounter = 0;
    
    private readonly string[] _easyQuestions = new[]
    {
        "What is your partner's favorite color?",
        "Do they prefer coffee or tea?",
        "What is their favorite season?"
    };
    
    private readonly string[] _mediumQuestions = new[]
    {
        "What is your partner's favorite food?",
        "Where was your first date?",
        "What is your partner's dream vacation destination?",
        "What time do they usually go to bed?"
    };
    
    private readonly string[] _hardQuestions = new[]
    {
        "What was their most defining life experience?",
        "How do they approach conflict resolution?",
        "What was their childhood ambition?"
    };

    private readonly Dictionary<string, string> _predefinedAnswers = new()
    {
        { "What is your partner's favorite color?", "Blue" },
        { "What is your partner's favorite food?", "Pizza" },
        { "Where was your first date?", "Restaurant" },
        { "What is your partner's dream vacation destination?", "Paris" }
    };

    // Updated return type from Task<string> to Task<Question>
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
        // Return a Question object with a default category
        var question = new Question { Text = questionText, Category = QuestionCategory.Preferences }; // Placeholder category
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
