using PoCoupleQuiz.Core.Services;
using Xunit;

namespace PoCoupleQuiz.Tests;

public class QuestionServiceIntegrationTests
{
    private readonly IQuestionService _questionService;

    public QuestionServiceIntegrationTests()
    {
        _questionService = new MockQuestionService();
    }

    [Fact]
    public async Task GenerateQuestion_ShouldReturnNonEmptyQuestion()
    {
        // Act
        var question = await _questionService.GenerateQuestionAsync();

        // Assert
        Assert.NotNull(question);
        Assert.NotEmpty(question);
        Assert.Contains("partner", question);
    }

    [Theory]
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

    [Fact]
    public async Task GenerateQuestion_ShouldReturnDifferentQuestionsOnMultipleCalls()
    {
        // Act
        var question1 = await _questionService.GenerateQuestionAsync();
        var question2 = await _questionService.GenerateQuestionAsync();
        var question3 = await _questionService.GenerateQuestionAsync();

        // Assert
        Assert.NotEqual(question1, question2);
        Assert.NotEqual(question2, question3);
        Assert.NotEqual(question1, question3);
    }

    [Fact]
    public async Task GenerateQuestion_ShouldReturnRelationshipRelatedQuestions()
    {
        // Act
        var question = await _questionService.GenerateQuestionAsync();
        var questionLower = question.ToLower();

        // Assert
        Assert.Contains("partner", questionLower);
        Assert.Contains("favorite", questionLower);
    }
} 