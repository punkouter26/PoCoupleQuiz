using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

public interface ITeamService
{
    Task<Team?> GetTeamAsync(string teamName);
    Task<IEnumerable<Team>> GetAllTeamsAsync();
    Task SaveTeamAsync(Team team);
    Task UpdateTeamStatsAsync(string teamName, int score, int questionsAnswered = 0, int correctAnswers = 0);
}
