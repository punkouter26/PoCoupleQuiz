namespace PoCoupleQuiz.Core.Models;

public class Game
{
    public List<Player> Players { get; set; } = new();
    public Player? KingPlayer => Players.FirstOrDefault(p => p.IsKingPlayer);
    public List<GameQuestion> Questions { get; set; } = new();
    public int CurrentRound { get; set; }
    public bool IsGameOver => CurrentRound >= 5; // Increased rounds since we have more players
    
    public int MinimumPlayers => 2; // At least 1 king and 1 guessing player
    public bool HasEnoughPlayers => Players.Count >= MinimumPlayers;
    public bool HasKingPlayer => KingPlayer != null;
    
    public Dictionary<string, int> GetScoreboard()
    {
        var scores = new Dictionary<string, int>();
        foreach (var player in Players.Where(p => !p.IsKingPlayer))
        {
            scores[player.Name] = player.Score;
        }
        return scores.OrderByDescending(s => s.Value)
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public void AddPlayer(Player player)
    {
        // Ensure only one king player
        if (player.IsKingPlayer && HasKingPlayer)
        {
            throw new InvalidOperationException("Game already has a king player");
        }
        Players.Add(player);
    }

    public bool AllPlayersAnswered(int roundIndex)
    {
        if (roundIndex >= Questions.Count) return false;
        
        var question = Questions[roundIndex];
        return Players.Where(p => !p.IsKingPlayer)
                     .All(p => question.HasPlayerAnswered(p.Name));
    }

    public void UpdateScores(int roundIndex)
    {
        if (roundIndex >= Questions.Count) return;
        
        var question = Questions[roundIndex];
        foreach (var player in Players.Where(p => !p.IsKingPlayer))
        {
            if (question.HasPlayerMatched(player.Name))
            {
                player.Score++;
                player.TotalCorrectGuesses++;
            }
            player.TotalGamesPlayed++;
        }
    }
}
