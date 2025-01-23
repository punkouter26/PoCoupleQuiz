using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

public interface ITeamService
{
    Task<Team?> GetTeamAsync(string teamName);
    Task SaveTeamAsync(Team team);
    Task UpdateTeamStatsAsync(string teamName, GameMode mode, int score);
} 