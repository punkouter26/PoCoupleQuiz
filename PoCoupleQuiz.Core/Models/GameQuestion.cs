namespace PoCoupleQuiz.Core.Models;

public class GameQuestion
{
    public string Question { get; set; } = string.Empty;
    public string KingPlayerAnswer { get; set; } = string.Empty;
    public Dictionary<string, string> PlayerAnswers { get; set; } = new();  // Key: PlayerName, Value: Answer
    public List<string> PlayersMatched { get; set; } = new();  // List of player names who matched the king's answer

    public bool HasPlayerAnswered(string playerName) => PlayerAnswers.ContainsKey(playerName);
    public bool HasPlayerMatched(string playerName) => PlayersMatched.Contains(playerName);

    public void RecordPlayerAnswer(string playerName, string answer)
    {
        PlayerAnswers[playerName] = answer;
        if (answer.Equals(KingPlayerAnswer, StringComparison.OrdinalIgnoreCase))
        {
            PlayersMatched.Add(playerName);
        }
    }
}
