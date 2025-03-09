namespace PoCoupleQuiz.Core.Services;

public interface IQuestionService
{
    Task<string> GenerateQuestionAsync(string? difficulty = null);
    Task<bool> CheckAnswerSimilarityAsync(string answer1, string answer2);
    Task<string> GenerateAnswerAsync(string question);
}