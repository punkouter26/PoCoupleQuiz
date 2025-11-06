using Xunit;
using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Tests.Utilities;

namespace PoCoupleQuiz.Tests;

/// <summary>
/// Unit tests for QuestionService
/// </summary>
public class QuestionServiceTests
{
    private readonly IQuestionService _questionService;

    public QuestionServiceTests()
    {
        _questionService = new Tests.Utilities.MockQuestionService();
    }

    [Trait("Category", "Unit")]
    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateQuestion_MockService_ReturnsNonEmptyQuestion()
    {
        // Act
        var question = await _questionService.GenerateQuestionAsync();

        // Assert
        Assert.NotNull(question);
        Assert.NotNull(question.Text); // Check the object itself
        Assert.NotEmpty(question.Text); // Check the Text property
    }

    [Trait("Category", "Unit")]
    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("red", "red", true)]
    [InlineData("blue", "red", false)]
    [InlineData("pizza", "Pizza", true)]
    [InlineData("I love pizza", "Pizza is my favorite", true)]
    [InlineData("January 1st", "1st of January", true)]
    [InlineData("completely different answers", "not even close", false)]
    [InlineData("", "", false)]
    [InlineData("answer1", "", false)]
    [InlineData("", "answer2", false)]
    [InlineData(" ", " ", false)]
    public async Task CheckAnswerSimilarity_VariousInputs_MatchesCorrectly(string answer1, string answer2, bool expectedMatch)
    {
        // Act
        var isMatch = await _questionService.CheckAnswerSimilarityAsync(answer1, answer2);

        // Assert
        Assert.Equal(expectedMatch, isMatch);
    }

    [Trait("Category", "Unit")]
    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateQuestion_MultipleCalls_ReturnsDifferentQuestions()
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

    [Trait("Category", "Unit")]
    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateQuestion_AfterAllQuestions_CyclesBackToStart()
    {
        // Act
        var firstRoundQuestions = new[]
        {
            await _questionService.GenerateQuestionAsync(),
            await _questionService.GenerateQuestionAsync(),
            await _questionService.GenerateQuestionAsync(),
            await _questionService.GenerateQuestionAsync()
        };

        var secondRoundQuestions = new[]
        {
            await _questionService.GenerateQuestionAsync(),
            await _questionService.GenerateQuestionAsync(),
            await _questionService.GenerateQuestionAsync(),
            await _questionService.GenerateQuestionAsync()
        };

        // Assert
        // Compare the Text property of the Question objects
        Assert.Equal(firstRoundQuestions.Select(q => q.Text), secondRoundQuestions.Select(q => q.Text));
    }
}