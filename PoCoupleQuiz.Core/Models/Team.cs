namespace PoCoupleQuiz.Core.Models;

public class Team
{
    public string Name { get; set; } = string.Empty;
    public int Wins { get; set; }
    public int Losses { get; set; }
    
    // For Azure Table Storage
    public string PartitionKey => "Team";
    public string RowKey => Name.ToLowerInvariant();
} 