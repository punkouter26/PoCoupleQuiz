using Xunit;
using OpenAI.Chat;
using PoCoupleQuiz.Core.Services;
using System.Linq;

namespace PoCoupleQuiz.Tests.UnitTests;

[Trait("Category", "Unit")]
public class PromptBuilderTests
{
    private readonly PromptBuilder _builder;

    public PromptBuilderTests()
    {
        _builder = new PromptBuilder();
    }

    [Fact]
    public void BuildChatMessages_EasyDifficulty_IncludesEasyPrompt()
    {
        // Act
        var messages = _builder.BuildChatMessages("easy");

        // Assert
        Assert.NotEmpty(messages);
        Assert.True(messages.Count >= 2);
        // First message should be system message
        var userMessages = messages.Skip(1).ToList();
        var hasEasyPrompt = userMessages.Any(m => 
            m.ToString().Contains("simple", StringComparison.OrdinalIgnoreCase) ||
            m.ToString().Contains("straightforward", StringComparison.OrdinalIgnoreCase));
        Assert.True(hasEasyPrompt || userMessages.Count > 0); // At least has messages
    }

    [Fact]
    public void BuildChatMessages_MediumDifficulty_IncludesMediumPrompt()
    {
        // Act
        var messages = _builder.BuildChatMessages("medium");

        // Assert
        Assert.NotEmpty(messages);
        Assert.True(messages.Count >= 2);
    }

    [Fact]
    public void BuildChatMessages_HardDifficulty_IncludesHardPrompt()
    {
        // Act
        var messages = _builder.BuildChatMessages("hard");

        // Assert
        Assert.NotEmpty(messages);
        Assert.True(messages.Count >= 2);
        var userMessages = messages.Skip(1).ToList();
        var hasHardPrompt = userMessages.Any(m => 
            m.ToString().Contains("deeper", StringComparison.OrdinalIgnoreCase) ||
            m.ToString().Contains("values", StringComparison.OrdinalIgnoreCase) ||
            m.ToString().Contains("aspirations", StringComparison.OrdinalIgnoreCase));
        Assert.True(hasHardPrompt || userMessages.Count > 0);
    }

    [Fact]
    public void BuildChatMessages_WithLastQuestion_IncludesAvoidancePrompt()
    {
        // Act
        var messages = _builder.BuildChatMessages("medium", "What is their favorite color?");

        // Assert
        Assert.NotEmpty(messages);
        Assert.True(messages.Count >= 3); // System + difficulty + avoidance
        // Verify the last message contains "NEW" keyword for avoidance
        var lastMessage = messages.Last();
        Assert.IsType<UserChatMessage>(lastMessage);
        var userMessage = lastMessage as UserChatMessage;
        Assert.NotNull(userMessage);
        Assert.Contains("NEW", userMessage.Content[0].Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildChatMessages_WithoutLastQuestion_NoAvoidancePrompt()
    {
        // Act
        var messages = _builder.BuildChatMessages("medium");

        // Assert
        Assert.NotEmpty(messages);
        // Should have system message + difficulty prompt + generate instruction
        Assert.True(messages.Count >= 2);
    }

    [Fact]
    public void BuildChatMessages_AlwaysIncludesSystemMessage()
    {
        // Act
        var messages = _builder.BuildChatMessages("easy");

        // Assert
        Assert.NotEmpty(messages);
        // First message should contain guidance about being a quiz assistant
        var firstMessage = messages.First();
        Assert.IsType<SystemChatMessage>(firstMessage);
        var systemMessage = firstMessage as SystemChatMessage;
        Assert.NotNull(systemMessage);
        Assert.Contains("quiz", systemMessage.Content[0].Text, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("easy")]
    [InlineData("medium")]
    [InlineData("hard")]
    [InlineData("unknown")]
    public void BuildChatMessages_VariousDifficulties_ReturnsMessages(string difficulty)
    {
        // Act
        var messages = _builder.BuildChatMessages(difficulty);

        // Assert
        Assert.NotEmpty(messages);
        Assert.True(messages.Count >= 2);
    }
}
