using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Tests.UnitTests;

/// <summary>
/// Consolidated unit tests for IQuestionService implementations.
/// Reduced from 12 individual tests to 4 parameterized tests using Theory.
/// </summary>
[Trait("Category", "Unit")]
public class QuestionServiceInterfaceTests
{
    private readonly Mock<IQuestionService> _mockQuestionService;

    public QuestionServiceInterfaceTests()
    {
        _mockQuestionService = new Mock<IQuestionService>();
    }

    [Theory]
    [InlineData("easy", "What is their favorite color?", QuestionCategory.Preferences)]
    [InlineData("medium", "What is their dream vacation?", QuestionCategory.Future)]
    [InlineData("hard", "What is their biggest fear?", QuestionCategory.Values)]
    [InlineData(null, "What is their favorite food?", QuestionCategory.Preferences)]
    public async Task GenerateQuestionAsync_AllDifficulties_ReturnsValidQuestion(string? difficulty, string expectedText, QuestionCategory expectedCategory)
    {
        // Arrange
        var expectedQuestion = new Question { Text = expectedText, Category = expectedCategory };
        _mockQuestionService.Setup(s => s.GenerateQuestionAsync(difficulty))
            .ReturnsAsync(expectedQuestion);

        // Act
        var result = await _mockQuestionService.Object.GenerateQuestionAsync(difficulty);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Text);
        Assert.Equal(expectedCategory, result.Category);
    }

    [Theory]
    [InlineData("Paris", "Paris", true)]       // Identical
    [InlineData("PARIS", "paris", true)]       // Case insensitive
    [InlineData("Happy", "Joyful", true)]      // Similar meaning
    [InlineData("Paris", "London", false)]     // Different
    [InlineData("", "", false)]                // Empty
    public async Task CheckAnswerSimilarityAsync_VariousAnswers_ReturnsExpectedResult(
        string answer1, string answer2, bool expectedResult)
    {
        // Arrange
        _mockQuestionService.Setup(s => s.CheckAnswerSimilarityAsync(answer1, answer2))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _mockQuestionService.Object.CheckAnswerSimilarityAsync(answer1, answer2);

        // Assert
        Assert.Equal(expectedResult, result);
    }
}
