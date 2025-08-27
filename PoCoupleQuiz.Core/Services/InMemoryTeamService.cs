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

    public Task UpdateTeamStatsAsync(string teamName, GameMode gameMode, int score)
    {
        if (_teams.TryGetValue(teamName.ToLowerInvariant(), out var team))
        {
            if (gameMode == GameMode.KingPlayer)
            {
                if (score > team.HighScore)
                {
                    team.HighScore = score;
                }
            }
        }
        return Task.CompletedTask;
    }
}
