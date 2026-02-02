using PoCoupleQuiz.Core.Services;
using Xunit;
using PoCoupleQuiz.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PoCoupleQuiz.Tests.Utilities;

namespace PoCoupleQuiz.Tests;

public class QuestionServiceIntegrationTests
{
    private readonly IQuestionService _questionService;
    private readonly IConfiguration _configuration;
    private readonly Mock<ILogger<AzureOpenAIQuestionService>> _mockLogger;

    public QuestionServiceIntegrationTests()
    {
        _configuration = TestConfiguration.GetConfiguration();
        _mockLogger = new Mock<ILogger<AzureOpenAIQuestionService>>();
        
        // AzureOpenAIQuestionService now has prompt building and caching inlined
        _questionService = new AzureOpenAIQuestionService(
            _configuration, 
            _mockLogger.Object);
    }

    [Fact(Skip = "Requires Azure OpenAI API Key and Endpoint. Remove Skip to run.")]
    public async Task GenerateQuestion_ShouldReturnNonEmptyQuestion()
    {
        // Act
        var question = await _questionService.GenerateQuestionAsync();

        // Assert
        Assert.NotNull(question);
        Assert.NotNull(question.Text);
        Assert.NotEmpty(question.Text);
        // The content of the question will vary, so a general assertion is better
        Assert.True(question.Text.Length > 10);
    }

    [Theory(Skip = "Requires Azure OpenAI API Key and Endpoint. Remove Skip to run.")]
    [InlineData("red", "red", true)]
    [InlineData("blue", "red", false)]
    [InlineData("pizza", "Pizza", true)]
    [InlineData("I love pizza", "Pizza is my favorite", true)]
    [InlineData("January 1st", "1st of January", true)]
    [InlineData("completely different answers", "not even close", false)]
    public async Task CheckAnswerSimilarity_ShouldMatchSimilarAnswers(string answer1, string answer2, bool expectedMatch)
    {
        // Act
        var isMatch = await _questionService.CheckAnswerSimilarityAsync(answer1, answer2);

        // Assert
        Assert.Equal(expectedMatch, isMatch);
    }

    [Fact(Skip = "Requires Azure OpenAI API Key and Endpoint. Remove Skip to run.")]
    public async Task GenerateQuestion_ShouldReturnDifferentQuestionsOnMultipleCalls()
    {
        // Act
        var question1 = await _questionService.GenerateQuestionAsync();
        var question2 = await _questionService.GenerateQuestionAsync();
        var question3 = await _questionService.GenerateQuestionAsync();

        // Assert
        Assert.NotEqual(question1.Text, question2.Text);
        // Due to the nature of AI, it's possible for questions to be similar or even identical
        // if the prompt is very specific or the model is constrained.
        // For integration tests, we might relax this or add more calls to increase probability.
        // For now, we'll keep it as is, but acknowledge the potential for flakiness.
    }

    [Fact(Skip = "Requires Azure OpenAI API Key and Endpoint. Remove Skip to run.")]
    public async Task GenerateQuestion_ShouldReturnRelationshipRelatedQuestions()
    {
        // Act
        var question = await _questionService.GenerateQuestionAsync();
        var questionLower = question.Text.ToLower();

        // Assert
        // Assertions here should be more general as AI output can vary.
        // For example, check for common question patterns or minimum length.
        Assert.True(questionLower.Contains("what") || questionLower.Contains("who") || questionLower.Contains("where"));
    }
}
