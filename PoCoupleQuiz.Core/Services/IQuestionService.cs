namespace PoCoupleQuiz.Core.Services;

public interface IQuestionService
{
    Task<string> GenerateQuestionAsync();
    Task<bool> CheckAnswerSimilarityAsync(string answer1, string answer2);
} 