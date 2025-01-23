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
    
    // For Azure Table Storage
    public string PartitionKey => "Team";
    public string RowKey => Name.ToLowerInvariant();
}