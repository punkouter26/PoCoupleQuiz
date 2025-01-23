using Xunit;
using PoCoupleQuiz.Core.Services;

namespace PoCoupleQuiz.Tests;

public class QuestionServiceTests
{
    private readonly IQuestionService _questionService;

    public QuestionServiceTests()
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
    }

    [Theory]
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
    public async Task GenerateQuestion_ShouldCycleBackToPredefinedQuestions()
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
        Assert.Equal(firstRoundQuestions, secondRoundQuestions);
    }
} 