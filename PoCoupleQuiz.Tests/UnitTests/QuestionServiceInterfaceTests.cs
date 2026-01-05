using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Tests.UnitTests;

/// <summary>
/// Unit tests for IQuestionService implementations.
/// Tests the service's behavior for question generation and answer similarity checking.
/// </summary>
[Trait("Category", "Unit")]
public class QuestionServiceInterfaceTests
{
    private readonly Mock<IQuestionService> _mockQuestionService;

    public QuestionServiceInterfaceTests()
    {
        _mockQuestionService = new Mock<IQuestionService>();
    }

    [Fact]
    public async Task GenerateQuestionAsync_WithEasyDifficulty_ReturnsQuestion()
    {
        // Arrange
        var expectedQuestion = new Question { Text = "What is their favorite color?", Category = QuestionCategory.Preferences };
        _mockQuestionService.Setup(s => s.GenerateQuestionAsync("easy"))
            .ReturnsAsync(expectedQuestion);

        // Act
        var result = await _mockQuestionService.Object.GenerateQuestionAsync("easy");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("What is their favorite color?", result.Text);
        Assert.Equal(QuestionCategory.Preferences, result.Category);
    }

    [Fact]
    public async Task GenerateQuestionAsync_WithMediumDifficulty_ReturnsQuestion()
    {
        // Arrange
        var expectedQuestion = new Question { Text = "What is their dream vacation?", Category = QuestionCategory.Future };
        _mockQuestionService.Setup(s => s.GenerateQuestionAsync("medium"))
            .ReturnsAsync(expectedQuestion);

        // Act
        var result = await _mockQuestionService.Object.GenerateQuestionAsync("medium");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Text);
    }

    [Fact]
    public async Task GenerateQuestionAsync_WithHardDifficulty_ReturnsQuestion()
    {
        // Arrange
        var expectedQuestion = new Question { Text = "What is their biggest fear?", Category = QuestionCategory.Values };
        _mockQuestionService.Setup(s => s.GenerateQuestionAsync("hard"))
            .ReturnsAsync(expectedQuestion);

        // Act
        var result = await _mockQuestionService.Object.GenerateQuestionAsync("hard");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Text);
    }

    [Fact]
    public async Task GenerateQuestionAsync_WithNullDifficulty_ReturnsDefaultQuestion()
    {
        // Arrange
        var expectedQuestion = new Question { Text = "What is their favorite food?", Category = QuestionCategory.Preferences };
        _mockQuestionService.Setup(s => s.GenerateQuestionAsync(null))
            .ReturnsAsync(expectedQuestion);

        // Act
        var result = await _mockQuestionService.Object.GenerateQuestionAsync(null);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Text);
    }

    [Fact]
    public async Task CheckAnswerSimilarityAsync_IdenticalAnswers_ReturnsTrue()
    {
        // Arrange
        _mockQuestionService.Setup(s => s.CheckAnswerSimilarityAsync("Paris", "Paris"))
            .ReturnsAsync(true);

        // Act
        var result = await _mockQuestionService.Object.CheckAnswerSimilarityAsync("Paris", "Paris");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CheckAnswerSimilarityAsync_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        _mockQuestionService.Setup(s => s.CheckAnswerSimilarityAsync("PARIS", "paris"))
            .ReturnsAsync(true);

        // Act
        var result = await _mockQuestionService.Object.CheckAnswerSimilarityAsync("PARIS", "paris");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CheckAnswerSimilarityAsync_DifferentAnswers_ReturnsFalse()
    {
        // Arrange
        _mockQuestionService.Setup(s => s.CheckAnswerSimilarityAsync("Paris", "London"))
            .ReturnsAsync(false);

        // Act
        var result = await _mockQuestionService.Object.CheckAnswerSimilarityAsync("Paris", "London");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CheckAnswerSimilarityAsync_EmptyAnswers_ReturnsFalse()
    {
        // Arrange
        _mockQuestionService.Setup(s => s.CheckAnswerSimilarityAsync("", ""))
            .ReturnsAsync(false);

        // Act
        var result = await _mockQuestionService.Object.CheckAnswerSimilarityAsync("", "");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CheckAnswerSimilarityAsync_SimilarMeaning_ReturnsTrue()
    {
        // Arrange - Testing synonyms/similar meanings
        _mockQuestionService.Setup(s => s.CheckAnswerSimilarityAsync("Happy", "Joyful"))
            .ReturnsAsync(true);

        // Act
        var result = await _mockQuestionService.Object.CheckAnswerSimilarityAsync("Happy", "Joyful");

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("easy")]
    [InlineData("medium")]
    [InlineData("hard")]
    public async Task GenerateQuestionAsync_AllDifficulties_ReturnsValidQuestion(string difficulty)
    {
        // Arrange
        var expectedQuestion = new Question { Text = "Test question", Category = QuestionCategory.Relationships };
        _mockQuestionService.Setup(s => s.GenerateQuestionAsync(difficulty))
            .ReturnsAsync(expectedQuestion);

        // Act
        var result = await _mockQuestionService.Object.GenerateQuestionAsync(difficulty);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Text);
    }
}
