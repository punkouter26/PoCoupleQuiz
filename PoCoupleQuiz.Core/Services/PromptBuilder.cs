using OpenAI.Chat;
using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

/// <summary>
/// Service for building chat prompts for question generation.
/// </summary>
public interface IPromptBuilder
{
    /// <summary>
    /// Builds chat messages for question generation.
    /// </summary>
    List<ChatMessage> BuildChatMessages(string difficulty, string? lastQuestion = null);
}

public class PromptBuilder : IPromptBuilder
{
    public List<ChatMessage> BuildChatMessages(string difficulty, string? lastQuestion = null)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a creative quiz game assistant. Generate fun, appropriate questions for couples to answer about each other. Questions should be light-hearted and suitable for all audiences.")
        };

        var difficultyPrompt = difficulty.ToLowerInvariant() switch
        {
            "easy" => "Generate a simple, straightforward question about basic preferences or daily habits. Examples: 'What is their favorite color?', 'What do they usually eat for breakfast?'",
            "medium" => "Generate a moderately thoughtful question about interests, opinions, or memories. Examples: 'What is their dream vacation destination?', 'What movie could they watch over and over?'",
            "hard" => "Generate a deeper question about values, aspirations, or specific memories. Examples: 'What is their biggest fear?', 'What childhood memory do they treasure most?'",
            _ => "Generate a fun question about their preferences, habits, or interests."
        };

        messages.Add(new UserChatMessage(difficultyPrompt));

        if (!string.IsNullOrEmpty(lastQuestion))
        {
            messages.Add(new UserChatMessage($"Generate a NEW question that is different from this one: '{lastQuestion}'. Make it creative and engaging."));
        }
        else
        {
            messages.Add(new UserChatMessage("Generate ONE creative question. Return ONLY the question text, nothing else."));
        }

        return messages;
    }
}
