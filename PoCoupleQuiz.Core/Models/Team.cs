namespace PoCoupleQuiz.Core.Models;

public class Team
{
    public Team()
    {
        LastPlayed = DateTime.UtcNow;
    }

    public string Name { get; set; } = string.Empty;
    public int MultiplayerWins { get; set; }
    public int SinglePlayerHighScore { get; set; }
    public DateTime LastPlayed { get; set; }
    public int TotalQuestionsAnswered { get; set; }
    public int CorrectAnswers { get; set; }

    public double CorrectPercentage => TotalQuestionsAnswered > 0
        ? (double)CorrectAnswers / TotalQuestionsAnswered * 100
        : 0;

    // For Azure Table Storage
    public string PartitionKey => "Team";
    public string RowKey => Name.ToLowerInvariant();
}