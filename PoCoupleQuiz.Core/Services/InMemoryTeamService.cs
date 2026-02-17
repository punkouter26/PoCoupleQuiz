using PoCoupleQuiz.Core.Models;
using System.Collections.Concurrent;

namespace PoCoupleQuiz.Core.Services;

public class InMemoryTeamService : ITeamService
{
    private static readonly ConcurrentDictionary<string, Team> _teams = new();

    public Task<Team?> GetTeamAsync(string teamName)
    {
        var team = _teams.TryGetValue(teamName.ToLowerInvariant(), out var foundTeam) ? foundTeam : null;
        return Task.FromResult(team);
    }

    public Task<IEnumerable<Team>> GetAllTeamsAsync()
    {
        return Task.FromResult<IEnumerable<Team>>(_teams.Values.ToList());
    }

    public Task SaveTeamAsync(Team team)
    {
        _teams[team.Name.ToLowerInvariant()] = team;
        return Task.CompletedTask;
    }

    public Task UpdateTeamStatsAsync(string teamName, int score, int questionsAnswered = 0, int correctAnswers = 0)
    {
        var key = teamName.ToLowerInvariant();

        if (!_teams.TryGetValue(key, out var team))
        {
            // Create team if it doesn't exist
            team = new Team
            {
                Name = teamName,
                HighScore = 0,
                TotalQuestionsAnswered = 0,
                CorrectAnswers = 0,
                LastPlayed = DateTime.UtcNow
            };
            _teams[key] = team;
        }

        if (score > team.HighScore)
        {
            team.HighScore = score;
        }

        team.TotalQuestionsAnswered += questionsAnswered;
        team.CorrectAnswers += correctAnswers;
        team.LastPlayed = DateTime.UtcNow;

        return Task.CompletedTask;
    }
}