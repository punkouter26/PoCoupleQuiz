using Azure;
using Azure.Data.Tables;

namespace PoCoupleQuiz.Core.Models;

public class GameHistory : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Team1Name { get; set; } = string.Empty;
    public string Team2Name { get; set; } = string.Empty;
    public GameMode GameMode { get; set; }
    public int Team1Score { get; set; }
    public int Team2Score { get; set; }
    public int TotalQuestions { get; set; }
    public double AverageResponseTime { get; set; }
    public string CategoryStats { get; set; } = string.Empty; // JSON string of Dictionary<QuestionCategory, int>
    public string MatchedAnswers { get; set; } = string.Empty; // JSON string of List<string>
    
    public static string GenerateRowKey() => $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"
} 