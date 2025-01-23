namespace PoCoupleQuiz.Core.Models;

public class GameQuestion
{
    public string Question { get; set; } = string.Empty;
    public string Team1PartnerAAnswer { get; set; } = string.Empty;
    public string Team1PartnerBAnswer { get; set; } = string.Empty;
    public string Team2PartnerAAnswer { get; set; } = string.Empty;
    public string Team2PartnerBAnswer { get; set; } = string.Empty;
    public bool Team1Matched { get; set; }
    public bool Team2Matched { get; set; }
} 