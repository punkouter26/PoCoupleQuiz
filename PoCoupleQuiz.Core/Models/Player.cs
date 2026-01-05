namespace PoCoupleQuiz.Core.Models;

public class Player
{
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool IsKingPlayer { get; set; }

    // For tracking statistics
    public int TotalRoundsPlayed { get; set; }
    public int TotalCorrectGuesses { get; set; }
    public double GuessAccuracy => TotalRoundsPlayed > 0
        ? (double)TotalCorrectGuesses / TotalRoundsPlayed * 100
        : 0;
}
