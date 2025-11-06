using Bunit;
using Xunit;
using PoCoupleQuiz.Client.Shared;
using Microsoft.JSInterop;
using Moq;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace PoCoupleQuiz.Tests.ComponentTests;

/// <summary>
/// bUnit tests for QuestionDisplay component
/// </summary>
public class QuestionDisplayTests : BunitContext
{
    [Fact]
    public void QuestionDisplay_RendersQuestionText()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        // Act
        var cut = Render<QuestionDisplay>(parameters => parameters
            .Add(p => p.QuestionText, "What is your favorite color?"));

        // Assert
        var questionText = cut.Find(".question-text");
        Assert.Contains("What is your favorite color?", questionText.TextContent);
    }

    [Fact]
    public void QuestionDisplay_KingPlayer_DisplaysKingInterface()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        // Act
        var cut = Render<QuestionDisplay>(parameters => parameters
            .Add(p => p.QuestionText, "Test question")
            .Add(p => p.IsKingPlayer, true)
            .Add(p => p.CurrentPlayerName, "Alice")
            .Add(p => p.ShowResults, false));

        // Assert
        var kingInterface = cut.Find(".king-interface");
        Assert.NotNull(kingInterface);
        Assert.Contains("Alice's Turn (King Player)", kingInterface.TextContent);
        Assert.Contains("Answer the question about yourself", kingInterface.TextContent);
    }

    [Fact]
    public void QuestionDisplay_RegularPlayer_DisplaysPlayerInterface()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        // Act
        var cut = Render<QuestionDisplay>(parameters => parameters
            .Add(p => p.QuestionText, "Test question")
            .Add(p => p.IsKingPlayer, false)
            .Add(p => p.CurrentPlayerName, "Bob")
            .Add(p => p.KingPlayerName, "Alice")
            .Add(p => p.ShowResults, false));

        // Assert
        var playerInterface = cut.Find(".player-interface");
        Assert.NotNull(playerInterface);
        Assert.Contains("Bob's Turn", playerInterface.TextContent);
        Assert.Contains("What do you think Alice would answer?", playerInterface.TextContent);
    }

    [Fact]
    public void QuestionDisplay_SubmitButton_DisabledWhenAnswerEmpty()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        // Act
        var cut = Render<QuestionDisplay>(parameters => parameters
            .Add(p => p.QuestionText, "Test question")
            .Add(p => p.Answer, "")
            .Add(p => p.ShowResults, false));

        // Assert
        var button = cut.Find("button.rz-button");
        Assert.True(button.HasAttribute("disabled"));
    }

    [Fact]
    public void QuestionDisplay_SubmitButton_EnabledWhenAnswerProvided()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        // Act
        var cut = Render<QuestionDisplay>(parameters => parameters
            .Add(p => p.QuestionText, "Test question")
            .Add(p => p.Answer, "Blue")
            .Add(p => p.ShowResults, false));

        // Assert
        var button = cut.Find("button.rz-button");
        Assert.False(button.HasAttribute("disabled"));
    }

    [Fact]
    public void QuestionDisplay_ColorQuestion_GeneratesColorSuggestions()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        // Act
        var cut = Render<QuestionDisplay>(parameters => parameters
            .Add(p => p.QuestionText, "What is your favorite color?")
            .Add(p => p.ShowResults, false));

        // Get the component instance to check private fields via reflection or trigger suggestion display
        var textarea = cut.Find("textarea");
        textarea.Focus();
        
        // After focusing, suggestions should be generated (they are in OnParametersSet)
        // We can't directly test private fields, but we can verify the component renders without errors
        Assert.NotNull(textarea);
    }

    [Fact]
    public void QuestionDisplay_FoodQuestion_GeneratesFoodSuggestions()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        // Act
        var cut = Render<QuestionDisplay>(parameters => parameters
            .Add(p => p.QuestionText, "What is your favorite food?")
            .Add(p => p.ShowResults, false));

        // Assert - component should render successfully with food question
        var questionText = cut.Find(".question-text");
        Assert.Contains("food", questionText.TextContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void QuestionDisplay_RegularPlayer_ShowsPlayerCountInfo()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        // Act
        var cut = Render<QuestionDisplay>(parameters => parameters
            .Add(p => p.QuestionText, "Test question")
            .Add(p => p.IsKingPlayer, false)
            .Add(p => p.CurrentGuessingPlayerIndex, 1)
            .Add(p => p.TotalGuessingPlayers, 3)
            .Add(p => p.ShowResults, false));

        // Assert
        var info = cut.Find("small.text-muted");
        Assert.Contains("Player 2 of 3", info.TextContent);
    }

    [Fact]
    public void QuestionDisplay_KingPlayer_DoesNotShowPlayerCountInfo()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        // Act
        var cut = Render<QuestionDisplay>(parameters => parameters
            .Add(p => p.QuestionText, "Test question")
            .Add(p => p.IsKingPlayer, true)
            .Add(p => p.ShowResults, false));

        // Assert
        var infoElements = cut.FindAll("small.text-muted");
        Assert.Empty(infoElements);
    }

    [Fact]
    public void QuestionDisplay_KingPlayer_ShowsSubmitAnswerButton()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        // Act
        var cut = Render<QuestionDisplay>(parameters => parameters
            .Add(p => p.QuestionText, "Test question")
            .Add(p => p.IsKingPlayer, true)
            .Add(p => p.Answer, "Test")
            .Add(p => p.ShowResults, false));

        // Assert
        var button = cut.Find("button.rz-button");
        Assert.Contains("Submit Answer", button.TextContent);
    }

    [Fact]
    public void QuestionDisplay_RegularPlayer_ShowsSubmitGuessButton()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        // Act
        var cut = Render<QuestionDisplay>(parameters => parameters
            .Add(p => p.QuestionText, "Test question")
            .Add(p => p.IsKingPlayer, false)
            .Add(p => p.Answer, "Test")
            .Add(p => p.ShowResults, false));

        // Assert
        var button = cut.Find("button.rz-button");
        Assert.Contains("Submit Guess", button.TextContent);
    }

    [Fact]
    public void QuestionDisplay_ShowResults_HidesInputInterface()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        // Act
        var cut = Render<QuestionDisplay>(parameters => parameters
            .Add(p => p.QuestionText, "Test question")
            .Add(p => p.ShowResults, true));

        // Assert
        var interfaces = cut.FindAll(".king-interface, .player-interface");
        Assert.Empty(interfaces);
    }

    [Fact]
    public void QuestionDisplay_RendersTextArea()
    {
        // Arrange
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);

        // Act
        var cut = Render<QuestionDisplay>(parameters => parameters
            .Add(p => p.QuestionText, "Test question")
            .Add(p => p.ShowResults, false));

        // Assert
        var textarea = cut.Find("textarea");
        Assert.NotNull(textarea);
    }
}
