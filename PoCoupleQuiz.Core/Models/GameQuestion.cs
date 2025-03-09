namespace PoCoupleQuiz.Core.Models;

public class GameQuestion
{
    public string Question { get; set; } = string.Empty;
    public QuestionCategory Category { get; set; }
    public string KingPlayerAnswer { get; set; } = string.Empty;
    public Dictionary<string, string> PlayerAnswers { get; set; } = new();
    public HashSet<string> PlayersMatched { get; set; } = new();

    public bool HasPlayerAnswered(string playerName)
    {
        return PlayerAnswers.ContainsKey(playerName);
    }

    public bool HasPlayerMatched(string playerName)
    {
        return PlayersMatched.Contains(playerName);
    }

    public void RecordPlayerAnswer(string playerName, string answer)
    {
        PlayerAnswers[playerName] = answer;
    }

    public void MarkPlayerAsMatched(string playerName)
    {
        PlayersMatched.Add(playerName);
    }
}
