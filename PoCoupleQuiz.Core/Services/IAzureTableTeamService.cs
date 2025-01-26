using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

public interface IAzureTableTeamService : ITeamService
{
    Task<IEnumerable<Team>> GetAllTeamsAsync();
} 