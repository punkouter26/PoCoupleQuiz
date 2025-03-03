using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;

namespace PoCoupleQuiz.Core.Services;

public class AzureOpenAIQuestionService : IQuestionService
{
    private readonly OpenAIClient _client;
    private readonly string _deploymentName;
    private string _lastQuestion = string.Empty; // Track the last question

    public AzureOpenAIQuestionService(IConfiguration configuration)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"] ?? throw new ArgumentNullException("AzureOpenAI:Endpoint");
        var key = configuration["AzureOpenAI:Key"] ?? throw new ArgumentNullException("AzureOpenAI:Key");
        _deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? throw new ArgumentNullException("AzureOpenAI:DeploymentName");

        _client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
    }

    public async Task<string> GenerateQuestionAsync()
    {
        var chatOptions = new ChatCompletionsOptions
        {
            Messages =
            {
                new ChatMessage(ChatRole.System, "You are a quiz game host. Generate fun, appropriate questions about someone's personal habits, preferences, and daily routines. These questions should help determine how well others know this person. Questions should be specific and have clear answers. For example: 'What is their favorite breakfast food?', 'What time do they usually go to bed?', 'What's their most used app on their phone?'"),
                new ChatMessage(ChatRole.User, $"Generate a single question that would reveal how well someone knows the person being asked about. Do not generate this question: {_lastQuestion}")
            },
            MaxTokens = 100,
            Temperature = 0.7f,
            NucleusSamplingFactor = 0.95f,
            FrequencyPenalty = 0.8f, // Increased to reduce repetition
            PresencePenalty = 0.8f   // Increased to reduce repetition
        };

        var response = await _client.GetChatCompletionsAsync(_deploymentName, chatOptions);
        var newQuestion = response.Value.Choices[0].Message.Content.Trim();
        _lastQuestion = newQuestion; // Store the new question
        return newQuestion;
    }

    public async Task<bool> CheckAnswerSimilarityAsync(string answer1, string answer2)
    {
        var chatOptions = new ChatCompletionsOptions
        {
            Messages =
            {
                new ChatMessage(ChatRole.System, "You are a quiz game judge. Your task is to determine if a player's guess matches what the main player answered about themselves. Consider semantic similarity and be somewhat lenient - answers don't need to be word-for-word identical to be considered a match. Answer with just 'yes' or 'no'."),
                new ChatMessage(ChatRole.User, $"Do these answers match?\nMain player's answer: {answer1}\nGuessing player's answer: {answer2}")
            },
            MaxTokens = 10,
            Temperature = 0.3f,
            NucleusSamplingFactor = 0.5f,
            FrequencyPenalty = 0,
            PresencePenalty = 0
        };

        var response = await _client.GetChatCompletionsAsync(_deploymentName, chatOptions);
        var result = response.Value.Choices[0].Message.Content.Trim().ToLower();
        return result.Contains("yes");
    }

    public async Task<string> GenerateAnswerAsync(string question)
    {
        var chatOptions = new ChatCompletionsOptions
        {
            Messages =
            {
                new ChatMessage(ChatRole.System, "You are an AI player in a quiz game. Generate a plausible and realistic answer about someone's personal habits or preferences. Keep answers concise and specific."),
                new ChatMessage(ChatRole.User, $"Generate an answer to this question: {question}")
            },
            MaxTokens = 50,
            Temperature = 0.6f,
            NucleusSamplingFactor = 0.9f,
            FrequencyPenalty = 0,
            PresencePenalty = 0
        };

        var response = await _client.GetChatCompletionsAsync(_deploymentName, chatOptions);
        return response.Value.Choices[0].Message.Content.Trim();
    }
}
