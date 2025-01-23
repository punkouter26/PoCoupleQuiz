using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;

namespace PoCoupleQuiz.Core.Services;

public class AzureOpenAIQuestionService : IQuestionService
{
    private readonly OpenAIClient _client;
    private readonly string _deploymentName;

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
                new ChatMessage(ChatRole.System, "You are a relationship quiz game host. Generate fun, appropriate questions that one partner should answer about the other partner. The questions should be specific and have clear answers."),
                new ChatMessage(ChatRole.User, "Generate a single question.")
            },
            MaxTokens = 100,
            Temperature = 0.7f,
            NucleusSamplingFactor = 0.95f,
            FrequencyPenalty = 0,
            PresencePenalty = 0
        };

        var response = await _client.GetChatCompletionsAsync(_deploymentName, chatOptions);
        return response.Value.Choices[0].Message.Content.Trim();
    }

    public async Task<bool> CheckAnswerSimilarityAsync(string answer1, string answer2)
    {
        var chatOptions = new ChatCompletionsOptions
        {
            Messages =
            {
                new ChatMessage(ChatRole.System, "You are a relationship quiz game judge. Your task is to determine if two answers mean the same thing. Answer with just 'yes' or 'no'."),
                new ChatMessage(ChatRole.User, $"Are these answers similar enough to be considered the same?\nAnswer 1: {answer1}\nAnswer 2: {answer2}")
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
} 