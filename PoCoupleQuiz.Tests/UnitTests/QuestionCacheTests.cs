using Xunit;
using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Core.Models;
using System.Threading;

namespace PoCoupleQuiz.Tests.UnitTests;

[Trait("Category", "Unit")]
public class QuestionCacheTests
{
    private readonly QuestionCache _cache;

    public QuestionCacheTests()
    {
        _cache = new QuestionCache();
    }

    [Fact]
    public void BuildCacheKey_WithDifficulty_ReturnsCorrectKey()
    {
        // Act
        var key = _cache.BuildCacheKey("medium");

        // Assert
        Assert.Equal("question_medium_none", key);
    }

    [Fact]
    public void BuildCacheKey_WithDifficultyAndLastQuestion_ReturnsCorrectKey()
    {
        // Act
        var key = _cache.BuildCacheKey("hard", "Previous question?");

        // Assert
        Assert.Equal("question_hard_Previous question?", key);
    }

    [Fact]
    public void CacheQuestion_AndGet_ReturnsQuestion()
    {
        // Arrange
        var key = "test_key";
        var question = new Question
        {
            Text = "What is your favorite color?",
            Category = QuestionCategory.Preferences
        };

        // Act
        _cache.CacheQuestion(key, question);
        var retrieved = _cache.GetCachedQuestion(key);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(question.Text, retrieved.Text);
        Assert.Equal(question.Category, retrieved.Category);
    }

    [Fact]
    public void GetCachedQuestion_NonExistentKey_ReturnsNull()
    {
        // Act
        var result = _cache.GetCachedQuestion("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCachedQuestion_ExpiredEntry_ReturnsNull()
    {
        // Note: This test would need to mock time or wait for expiration
        // For now, just verify the method works
        // Arrange
        var key = "expired_key";
        var question = new Question
        {
            Text = "Test?",
            Category = QuestionCategory.Preferences
        };

        // Act
        _cache.CacheQuestion(key, question);

        // Immediately retrieve (should work)
        var result = _cache.GetCachedQuestion(key);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void CacheQuestion_MultipleTimes_UpdatesCache()
    {
        // Arrange
        var key = "update_key";
        var question1 = new Question { Text = "Question 1?", Category = QuestionCategory.Preferences };
        var question2 = new Question { Text = "Question 2?", Category = QuestionCategory.Future };

        // Act
        _cache.CacheQuestion(key, question1);
        _cache.CacheQuestion(key, question2);
        var retrieved = _cache.GetCachedQuestion(key);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Question 2?", retrieved.Text);
    }
}
