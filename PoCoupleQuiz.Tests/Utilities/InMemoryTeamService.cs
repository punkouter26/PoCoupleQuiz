using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Core.Services;
using System.Collections.Concurrent;

namespace PoCoupleQuiz.Tests.Utilities
{
    public class InMemoryTeamService : ITeamService
    {
        private readonly ConcurrentDictionary<string, Team> _teams = new();

        public Task<Team?> GetTeamAsync(string teamName)
        {
            _teams.TryGetValue(teamName, out var team);
            return Task.FromResult(team);
        }

        public Task<IEnumerable<Team>> GetAllTeamsAsync()
        {
            return Task.FromResult(_teams.Values.AsEnumerable());
        }

        public Task SaveTeamAsync(Team team)
        {
            _teams.AddOrUpdate(team.Name, team, (key, oldValue) => team);
            return Task.CompletedTask;
        }

        public Task UpdateTeamStatsAsync(string teamName, GameMode gameMode, int score)
        {
            if (_teams.TryGetValue(teamName, out var team))
            {                team.TotalQuestionsAnswered++;
                if (score > 0)
                {
                    team.CorrectAnswers++;
                }
                team.LastPlayed = DateTime.UtcNow;
                // CorrectPercentage is calculated automatically
            }
            return Task.CompletedTask;
        }
    }
}
