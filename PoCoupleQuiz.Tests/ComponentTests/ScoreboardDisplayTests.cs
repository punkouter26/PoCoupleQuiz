using Bunit;
using Xunit;
using PoCoupleQuiz.Client.Shared;
using Microsoft.JSInterop;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace PoCoupleQuiz.Tests.ComponentTests;

/// <summary>
/// bUnit tests for ScoreboardDisplay component
/// </summary>
public class ScoreboardDisplayTests : Bunit.TestContext
{
    [Fact]
    public void ScoreboardDisplay_WithNoScores_DisplaysNoScoresMessage()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        // Act
        var cut = Render<ScoreboardDisplay>(parameters => parameters
            .Add(p => p.Scoreboard, new Dictionary<string, int>()));

        // Assert
        var paragraph = cut.Find("p");
        Assert.Contains("No scores to display", paragraph.TextContent);
    }

    [Fact]
    public void ScoreboardDisplay_WithScores_DisplaysAllPlayers()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        var scoreboard = new Dictionary<string, int>
        {
            { "Player1", 10 },
            { "Player2", 5 },
            { "Player3", 15 }
        };

        // Act
        var cut = Render<ScoreboardDisplay>(parameters => parameters
            .Add(p => p.Scoreboard, scoreboard));

        // Assert
        var playerNames = cut.FindAll(".player-name");
        Assert.Equal(3, playerNames.Count);
        Assert.Contains(playerNames, el => el.TextContent == "Player1");
        Assert.Contains(playerNames, el => el.TextContent == "Player2");
        Assert.Contains(playerNames, el => el.TextContent == "Player3");
    }

    [Fact]
    public void ScoreboardDisplay_OrdersPlayersByScoreDescending()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        var scoreboard = new Dictionary<string, int>
        {
            { "Player1", 5 },
            { "Player2", 15 },
            { "Player3", 10 }
        };

        // Act
        var cut = Render<ScoreboardDisplay>(parameters => parameters
            .Add(p => p.Scoreboard, scoreboard));

        // Assert
        var playerNames = cut.FindAll(".player-name");
        Assert.Equal("Player2", playerNames[0].TextContent); // Highest score (15)
        Assert.Equal("Player3", playerNames[1].TextContent); // Middle score (10)
        Assert.Equal("Player1", playerNames[2].TextContent); // Lowest score (5)
    }

    [Fact]
    public void ScoreboardDisplay_DisplaysCorrectScores()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        var scoreboard = new Dictionary<string, int>
        {
            { "Player1", 42 },
            { "Player2", 17 }
        };

        // Act
        var cut = Render<ScoreboardDisplay>(parameters => parameters
            .Add(p => p.Scoreboard, scoreboard));

        // Assert
        var scores = cut.FindAll(".player-score");
        Assert.Contains(scores, el => el.GetAttribute("data-score") == "42");
        Assert.Contains(scores, el => el.GetAttribute("data-score") == "17");
    }

    [Fact]
    public void ScoreboardDisplay_WithUpdatedScore_AppliesUpdatedClass()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        var previousScoreboard = new Dictionary<string, int>
        {
            { "Player1", 5 }
        };

        var currentScoreboard = new Dictionary<string, int>
        {
            { "Player1", 10 }
        };

        // Act
        var cut = Render<ScoreboardDisplay>(parameters => parameters
            .Add(p => p.Scoreboard, currentScoreboard)
            .Add(p => p.PreviousScoreboard, previousScoreboard));

        // Assert
        var updatedItem = cut.Find(".score-updated");
        Assert.NotNull(updatedItem);
        var playerName = updatedItem.QuerySelector(".player-name");
        Assert.Equal("Player1", playerName?.TextContent);
    }

    [Fact]
    public void ScoreboardDisplay_WithNoScoreChange_DoesNotApplyUpdatedClass()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        var scoreboard = new Dictionary<string, int>
        {
            { "Player1", 5 }
        };

        // Act
        var cut = Render<ScoreboardDisplay>(parameters => parameters
            .Add(p => p.Scoreboard, scoreboard)
            .Add(p => p.PreviousScoreboard, scoreboard));

        // Assert
        var updatedItems = cut.FindAll(".score-updated");
        Assert.Empty(updatedItems);
    }

    [Fact]
    public void ScoreboardDisplay_RendersHeader()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        var scoreboard = new Dictionary<string, int>
        {
            { "Player1", 10 }
        };

        // Act
        var cut = Render<ScoreboardDisplay>(parameters => parameters
            .Add(p => p.Scoreboard, scoreboard));

        // Assert
        var header = cut.Find("h4");
        Assert.Equal("Current Scores", header.TextContent);
    }
}
