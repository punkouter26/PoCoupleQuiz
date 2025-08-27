namespace PoCoupleQuiz.Core.Models;

public class Player
{
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool IsKingPlayer { get; set; }

    // For tracking statistics
    public int TotalGamesPlayed { get; set; }
    public int TotalCorrectGuesses { get; set; }
    public double GuessAccuracy => TotalGamesPlayed > 0
        ? (double)TotalCorrectGuesses / TotalGamesPlayed * 100
        : 0;
}
