using PoCoupleQuiz.Core.Services;
using Xunit;
using PoCoupleQuiz.Core.Models; // Added using

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
        Assert.NotNull(question.Text); // Check the object
        Assert.NotEmpty(question.Text); // Check the Text property
        Assert.Contains("partner", question.Text); // Assert on the Text property
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
        Assert.NotEqual(question1.Text, question2.Text);
        Assert.NotEqual(question2.Text, question3.Text);
        Assert.NotEqual(question1.Text, question3.Text);
    }

    [Fact]
    public async Task GenerateQuestion_ShouldReturnRelationshipRelatedQuestions()
    {
        // Act
        var question = await _questionService.GenerateQuestionAsync();
        var questionLower = question.Text.ToLower(); // Use Text property

        // Assert
        Assert.Contains("partner", questionLower);
        // Note: The mock service might not always return "favorite" questions, 
        // this assertion might be too strict depending on the mock implementation.
        // Keeping it for now, but might need adjustment if tests become flaky.
        Assert.Contains("favorite", questionLower); 
    }
}
