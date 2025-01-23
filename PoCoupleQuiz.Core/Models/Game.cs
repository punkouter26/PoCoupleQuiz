namespace PoCoupleQuiz.Core.Models;

public class Game
{
    public Team Team1 { get; set; } = new();
    public Team Team2 { get; set; } = new();
    public List<GameQuestion> Questions { get; set; } = new();
    public int CurrentRound { get; set; }
    public int Team1Score { get; set; }
    public int Team2Score { get; set; }
    public bool IsGameOver => CurrentRound >= 3;
} 