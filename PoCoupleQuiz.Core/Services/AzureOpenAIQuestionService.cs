using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace PoCoupleQuiz.Core.Services;

public class AzureOpenAIQuestionService : IQuestionService
{
    private readonly OpenAIClient _client;
    private readonly string _deploymentName;
    private string _lastQuestion = string.Empty; // Track the last question
    private readonly Dictionary<string, string> _promptCache = new(); // Cache for prompts
    private readonly string[] _fallbackQuestions = new[]
    {
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
    };
    private readonly Random _random = new Random();

    public AzureOpenAIQuestionService(IConfiguration configuration)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"] ?? throw new ArgumentNullException("AzureOpenAI:Endpoint");
        var key = configuration["AzureOpenAI:Key"] ?? throw new ArgumentNullException("AzureOpenAI:Key");
        _deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? throw new ArgumentNullException("AzureOpenAI:DeploymentName");

        _client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
        
        Console.WriteLine($"AzureOpenAIQuestionService initialized with endpoint: {endpoint} and deployment: {_deploymentName}");
    }

    public async Task<string> GenerateQuestionAsync(string? difficulty = null)
    {
        // Create cache key
        string cacheKey = $"question_{difficulty ?? "medium"}_{_lastQuestion}";
        
        // Check if we have a cached prompt
        if (_promptCache.ContainsKey(cacheKey))
        {
            var cachedQuestion = _promptCache[cacheKey];
            _lastQuestion = cachedQuestion;
            Console.WriteLine($"Using cached question: {cachedQuestion}");
            return cachedQuestion;
        }

        try 
        {
            var systemPrompt = difficulty?.ToLower() switch
            {
                "easy" => "You are a quiz game host. Generate simple, fun, and easily answerable questions about someone's personal habits, preferences, and daily routines. The questions should be straightforward with clear answers. For example: 'What is their favorite color?', 'Do they prefer coffee or tea?'",
                "hard" => "You are a quiz game host. Generate challenging and thought-provoking questions about someone's personal values, complex preferences, and deeper characteristics. These questions should require deeper knowledge of the person. For example: 'What would they say was their most defining life experience?', 'What's their approach to handling conflict?'",
                _ => "You are a quiz game host. Generate fun, appropriate questions about someone's personal habits, preferences, and daily routines. These questions should help determine how well others know this person. Questions should be specific and have clear answers. For example: 'What is their favorite breakfast food?', 'What time do they usually go to bed?', 'What's their most used app on their phone?'"
            };

            var chatOptions = new ChatCompletionsOptions
            {
                Messages =
                {
                    new ChatMessage(ChatRole.System, systemPrompt),
                    new ChatMessage(ChatRole.User, $"Generate a single question that would reveal how well someone knows the person being asked about. Do not generate this question: {_lastQuestion}")
                },
                MaxTokens = 100,
                Temperature = 0.7f,
                NucleusSamplingFactor = 0.95f,
                FrequencyPenalty = 0.8f, 
                PresencePenalty = 0.8f   
            };

            Console.WriteLine($"Sending request to Azure OpenAI with deployment: {_deploymentName}");
            var response = await _client.GetChatCompletionsAsync(_deploymentName, chatOptions);
            var newQuestion = response.Value.Choices[0].Message.Content.Trim();
            
            // Log success
            Console.WriteLine($"Successfully generated question via API: {newQuestion}");
            
            // Cache the result
            _promptCache[cacheKey] = newQuestion;
            
            _lastQuestion = newQuestion; // Store the new question
            return newQuestion;
        }
        catch (RequestFailedException ex)
        {
            // Log detailed error information
            Console.WriteLine($"Azure OpenAI API Error: Status={ex.Status}, Message={ex.Message}");
            
            // Provide more specific error handling based on status code
            if (ex.Status == (int)HttpStatusCode.TooManyRequests)
            {
                Console.WriteLine("Rate limit exceeded. Using fallback question.");
            }
            else if (ex.Status == (int)HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("Authentication failed. Check API key and endpoint.");
            }
            
            // Use a fallback question
            var fallbackQuestion = GetFallbackQuestion();
            _lastQuestion = fallbackQuestion;
            return fallbackQuestion;
        }
        catch (Exception ex)
        {
            // Log general exception
            Console.WriteLine($"Error generating question: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Use a fallback question
            var fallbackQuestion = GetFallbackQuestion();
            _lastQuestion = fallbackQuestion;
            return fallbackQuestion;
        }
    }

    private string GetFallbackQuestion()
    {
        return _fallbackQuestions[_random.Next(_fallbackQuestions.Length)];
    }

    public async Task<bool> CheckAnswerSimilarityAsync(string answer1, string answer2)
    {
        // Quick check for exact matches to avoid API call
        if (string.Equals(answer1, answer2, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        // Create cache key
        string cacheKey = $"similarity_{answer1}_{answer2}".ToLowerInvariant();
        
        // Check if we have a cached result
        if (_promptCache.ContainsKey(cacheKey))
        {
            return bool.Parse(_promptCache[cacheKey]);
        }

        try
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
            var isMatch = result.Contains("yes");
            
            // Cache the result
            _promptCache[cacheKey] = isMatch.ToString();
            
            return isMatch;
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Error checking answer similarity: {ex.Message}");
            
            // Fallback to simple string comparison when API fails
            bool simpleMatch = answer1.ToLowerInvariant().Contains(answer2.ToLowerInvariant()) || 
                              answer2.ToLowerInvariant().Contains(answer1.ToLowerInvariant());
            return simpleMatch;
        }
    }

    public async Task<string> GenerateAnswerAsync(string question)
    {
        // Create cache key
        string cacheKey = $"answer_{question}";
        
        // Check if we have a cached answer
        if (_promptCache.ContainsKey(cacheKey))
        {
            return _promptCache[cacheKey];
        }

        try
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
            var answer = response.Value.Choices[0].Message.Content.Trim();
            
            // Cache the answer
            _promptCache[cacheKey] = answer;
            
            return answer;
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Error generating answer: {ex.Message}");
            
            // Return fallback answers based on question type
            if (question.Contains("favorite"))
                return "Chocolate ice cream";
            else if (question.Contains("hobby"))
                return "Reading novels";
            else if (question.Contains("time") || question.Contains("when"))
                return "Around 10 PM";
            else
                return "It depends on the day";
        }
    }
}
