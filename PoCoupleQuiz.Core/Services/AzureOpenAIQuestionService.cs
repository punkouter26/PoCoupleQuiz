using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;
using System.Net;
using PoCoupleQuiz.Core.Models;
using Polly;
using Polly.Retry;
using Microsoft.Extensions.Logging;
using System.ClientModel;

namespace PoCoupleQuiz.Core.Services;

/// <summary>
/// Question service implementation using Azure AI Foundry (Azure OpenAI) for generating
/// quiz questions and evaluating answer similarity.
/// </summary>
public class AzureOpenAIQuestionService : IQuestionService
{
    private readonly AzureOpenAIClient _client;
    private readonly string _deploymentName;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IQuestionCache _questionCache;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly ILogger<AzureOpenAIQuestionService> _logger;
    private string _lastQuestion = string.Empty;
    
    private readonly string[] _fallbackQuestions =
    [
        "What is their favorite food?",
        "What is their favorite movie?",
        "What is their dream vacation destination?",
        "What would they choose as their superpower if they could have one?",
        "What is their favorite way to relax after a long day?",
        "What is their favorite season of the year?",
        "What is their favorite type of music?",
        "What is their favorite book or author?",
        "What is their preferred morning beverage?",
        "What is their favorite hobby or pastime?"
    ];
    private readonly Random _random = new();

    /// <summary>
    /// Initializes the Azure AI Foundry question service with injected dependencies.
    /// </summary>
    public AzureOpenAIQuestionService(
        IConfiguration configuration,
        ILogger<AzureOpenAIQuestionService> logger,
        IPromptBuilder promptBuilder,
        IQuestionCache questionCache)
    {
        _logger = logger;
        _promptBuilder = promptBuilder;
        _questionCache = questionCache;

        var endpoint = configuration["AzureOpenAI:Endpoint"] 
            ?? throw new ArgumentNullException("AzureOpenAI:Endpoint", "Azure AI Foundry endpoint is required");
        var key = configuration["AzureOpenAI:Key"] 
            ?? throw new ArgumentNullException("AzureOpenAI:Key", "Azure AI Foundry API key is required");
        _deploymentName = configuration["AzureOpenAI:DeploymentName"] 
            ?? throw new ArgumentNullException("AzureOpenAI:DeploymentName", "Azure AI Foundry deployment name is required");

        _client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));

        // Configure Polly resilience pipeline for retry with exponential backoff
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<RequestFailedException>(ex =>
                    ex.Status == (int)HttpStatusCode.TooManyRequests ||
                    ex.Status == (int)HttpStatusCode.InternalServerError ||
                    ex.Status == (int)HttpStatusCode.ServiceUnavailable),
                Delay = TimeSpan.FromSeconds(1),
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    _logger.LogWarning(args.Outcome.Exception,
                        "Azure AI Foundry: Retrying due to {ExceptionType}. Attempt: {AttemptNumber}",
                        args.Outcome.Exception?.GetType().Name, args.AttemptNumber);
                    return default;
                }
            })
            .Build();

        _logger.LogInformation("Azure AI Foundry Question Service initialized - Endpoint: {Endpoint}, Deployment: {DeploymentName}",
            endpoint, _deploymentName);
    }

    /// <inheritdoc />
    public async Task<Question> GenerateQuestionAsync(string? difficulty = null)
    {
        var normalizedDifficulty = difficulty ?? "medium";
        var cacheKey = _questionCache.BuildCacheKey(normalizedDifficulty, _lastQuestion);

        // Check cache first
        var cachedQuestion = _questionCache.GetCachedQuestion(cacheKey);
        if (cachedQuestion != null)
        {
            _logger.LogDebug("Returning cached question: {QuestionText}", cachedQuestion.Text);
            _lastQuestion = cachedQuestion.Text;
            return cachedQuestion;
        }

        try
        {
            // Use injected PromptBuilder
            var messages = _promptBuilder.BuildChatMessages(normalizedDifficulty, _lastQuestion);

            _logger.LogInformation("Generating question via Azure AI Foundry - Difficulty: {Difficulty}", normalizedDifficulty);

            var response = await ExecuteWithRetryAsync(messages, new ChatCompletionOptions
            {
                MaxOutputTokenCount = 100,
                Temperature = 0.7f,
                TopP = 0.95f,
                FrequencyPenalty = 0.8f,
                PresencePenalty = 0.8f
            });

            var questionText = response.Value.Content[0].Text.Trim();
            var question = new Question { Text = questionText, Category = QuestionCategory.Preferences };

            // Cache the result
            _questionCache.CacheQuestion(cacheKey, question);
            _lastQuestion = questionText;

            _logger.LogInformation("Generated question: {QuestionText}", questionText);
            return question;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure AI Foundry API Error - Status: {Status}", ex.Status);
            return GetFallbackQuestion();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating question from Azure AI Foundry");
            return GetFallbackQuestion();
        }
    }

    /// <inheritdoc />
    public async Task<bool> CheckAnswerSimilarityAsync(string answer1, string answer2)
    {
        // Quick exact match check
        if (string.Equals(answer1, answer2, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(
                    "You are a quiz game judge. Determine if a player's guess matches the main player's answer. " +
                    "Consider semantic similarity - answers don't need to be word-for-word identical. " +
                    "Answer with just 'yes' or 'no'."),
                new UserChatMessage(
                    $"Do these answers match?\nMain player's answer: {answer1}\nGuessing player's answer: {answer2}")
            };

            var response = await ExecuteWithRetryAsync(messages, new ChatCompletionOptions
            {
                MaxOutputTokenCount = 10,
                Temperature = 0.3f,
                TopP = 0.5f
            });

            var result = response.Value.Content[0].Text.Trim().ToLowerInvariant();
            return result.Contains("yes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking answer similarity via Azure AI Foundry");
            
            // Fallback to simple substring matching
            return answer1.Contains(answer2, StringComparison.OrdinalIgnoreCase) ||
                   answer2.Contains(answer1, StringComparison.OrdinalIgnoreCase);
        }
    }

    private async Task<ClientResult<ChatCompletion>> ExecuteWithRetryAsync(
        List<ChatMessage> messages,
        ChatCompletionOptions options)
    {
        return await _resiliencePipeline.ExecuteAsync(async token =>
        {
            var chatClient = _client.GetChatClient(_deploymentName);
            return await chatClient.CompleteChatAsync(messages, options, token);
        });
    }

    private Question GetFallbackQuestion()
    {
        var fallbackText = _fallbackQuestions[_random.Next(_fallbackQuestions.Length)];
        _lastQuestion = fallbackText;
        _logger.LogWarning("Using fallback question: {FallbackText}", fallbackText);
        return new Question { Text = fallbackText, Category = QuestionCategory.Preferences };
    }
}
