namespace PoCoupleQuiz.Core.Models;

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}

public class Game
{
    public List<Player> Players { get; set; } = new();
    public int CurrentKingPlayerIndex { get; set; } = 0; // New property to track current king player
    public Player? KingPlayer => Players.Any() ? Players[CurrentKingPlayerIndex] : null; // Use index for KingPlayer
    public List<GameQuestion> Questions { get; set; } = new();
    public int CurrentRound { get; set; }
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
    public int MaxRounds => Difficulty switch
    {
        DifficultyLevel.Easy => 3,
        DifficultyLevel.Medium => 5,
        DifficultyLevel.Hard => 7,
        _ => 5
    };
    public bool IsGameOver => CurrentRound >= MaxRounds;

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
        Players.Add(player);
        // If this is the first player added and they are designated as king, set them as the initial king
        if (Players.Count == 1 && player.IsKingPlayer)
        {
            CurrentKingPlayerIndex = 0;
        }
    }

    public void SetNextKingPlayer()
    {
        // Move to the next player in the list to be the king
        CurrentKingPlayerIndex = (CurrentKingPlayerIndex + 1) % Players.Count;
        // Ensure the new king player is marked as such, and others are not
        foreach (var player in Players)
        {
            player.IsKingPlayer = (player == KingPlayer);
        }
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
