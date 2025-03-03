using System.Text.Json.Serialization;

namespace PoCoupleQuiz.Core.Models;

public class Question
{
    public string Text { get; set; } = string.Empty;
    public QuestionCategory Category { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum QuestionCategory
{
    [JsonPropertyName("relationships")]
    Relationships,

    [JsonPropertyName("hobbies")]
    Hobbies,

    [JsonPropertyName("childhood")]
    Childhood,

    [JsonPropertyName("future")]
    Future,

    [JsonPropertyName("preferences")]
    Preferences,

    [JsonPropertyName("values")]
    Values
} 