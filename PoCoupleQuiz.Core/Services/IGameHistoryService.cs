using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

public interface IGameHistoryService
{
    Task SaveGameHistoryAsync(GameHistory history);
    Task<IEnumerable<GameHistory>> GetTeamHistoryAsync(string teamName);
    Task<Dictionary<QuestionCategory, int>> GetTeamCategoryStatsAsync(string teamName);
    Task<List<string>> GetTopMatchedAnswersAsync(string teamName, int count = 10);
    Task<double> GetAverageResponseTimeAsync(string teamName);
}