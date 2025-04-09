using PoCoupleQuiz.Core.Models; // Added for Question model

namespace PoCoupleQuiz.Core.Services;

public interface IQuestionService
{
    Task<Question> GenerateQuestionAsync(string? difficulty = null); // Changed return type
    Task<bool> CheckAnswerSimilarityAsync(string answer1, string answer2);
    Task<string> GenerateAnswerAsync(string question);
}
