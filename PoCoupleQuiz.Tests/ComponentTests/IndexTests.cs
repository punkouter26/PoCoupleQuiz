using Bunit;
using Xunit;
using PoCoupleQuiz.Core.Services;
using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Components;
using PoCoupleQuiz.Core.Models;
using IndexPage = PoCoupleQuiz.Client.Pages.Index;

namespace PoCoupleQuiz.Tests.ComponentTests;

/// <summary>
/// bUnit tests for Index (Home) page component
/// </summary>
public class IndexTests : BunitContext
{
    private Mock<ITeamService> _mockTeamService = null!;
    private Mock<IGameStateService> _mockGameState = null!;
    private Mock<IJSRuntime> _mockJSRuntime = null!;
    private Mock<NavigationManager> _mockNavigationManager = null!;

    public IndexTests()
    {
        SetupMocks();
    }

    private void SetupMocks()
    {
        _mockTeamService = new Mock<ITeamService>();
        _mockGameState = new Mock<IGameStateService>();
        _mockJSRuntime = new Mock<IJSRuntime>();
        
        // Setup JSRuntime to return null for localStorage calls (no saved preferences)
        _mockJSRuntime.Setup(js => js.InvokeAsync<string>(
            "localStorage.getItem",
            It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        // Create a concrete NavigationManager mock
        var navManager = new TestNavigationManager();
        
        Services.AddSingleton(_mockTeamService.Object);
        Services.AddSingleton(_mockGameState.Object);
        Services.AddSingleton<IJSRuntime>(_mockJSRuntime.Object);
        Services.AddSingleton<NavigationManager>(navManager);
    }

    [Fact]
    public void Index_ShouldRenderCorrectly()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert
        var heading = cut.Find("h6");
        Assert.Contains("Game Setup", heading.TextContent);
    }

    [Fact]
    public void Index_DefaultPlayerCount_IsThree()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert
        var select = cut.Find("select");
        Assert.Equal("3", select.GetAttribute("value"));
    }

    [Fact]
    public void Index_RendersThreePlayerNameInputs_ByDefault()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert
        var inputs = cut.FindAll("input.form-control");
        Assert.Equal(3, inputs.Count);
    }

    [Fact]
    public void Index_FirstPlayerInput_HasKingPlaceholder()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert
        var firstInput = cut.FindAll("input.form-control")[0];
        Assert.Equal("King", firstInput.GetAttribute("placeholder"));
    }

    [Fact]
    public void Index_KingPlayerInput_HasHelpText()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert
        var helpText = cut.Find("small");
        Assert.Contains("King answers questions", helpText.TextContent);
    }

    [Fact]
    public void Index_StartGameButton_Exists()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert
        var button = cut.Find("button.btn-primary");
        Assert.Equal("START GAME", button.TextContent.Trim());
    }

    [Fact]
    public void Index_DifficultySelector_ExistsWithOptions()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert
        var difficultySelect = cut.FindAll("select")[1]; // Second select is difficulty
        var options = difficultySelect.QuerySelectorAll("option");
        
        Assert.Equal(3, options.Length);
        Assert.Contains(options, o => o.TextContent.Contains("Easy (3 rounds)"));
        Assert.Contains(options, o => o.TextContent.Contains("Medium (5 rounds)"));
        Assert.Contains(options, o => o.TextContent.Contains("Hard (7 rounds)"));
    }

    [Fact]
    public void Index_DefaultDifficulty_IsMedium()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert
        var difficultySelect = cut.FindAll("select")[1];
        Assert.Equal("Medium", difficultySelect.GetAttribute("value"));
    }

    [Fact]
    public void Index_RendersPlayerCountOptions()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert
        var playerCountSelect = cut.FindAll("select")[0];
        var options = playerCountSelect.QuerySelectorAll("option");
        
        Assert.Equal(4, options.Length); // 3, 4, 5, 6 players
        Assert.Contains(options, o => o.TextContent == "3 Players");
        Assert.Contains(options, o => o.TextContent == "4 Players");
        Assert.Contains(options, o => o.TextContent == "5 Players");
        Assert.Contains(options, o => o.TextContent == "6 Players");
    }

    [Fact]
    public void Index_HasGameSetupTitle()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert
        var title = cut.Find("h6");
        Assert.Contains("Game Setup", title.TextContent);
        Assert.Contains("🎲", title.TextContent);
    }

    [Fact]
    public void Index_RendersCard()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert
        var card = cut.Find(".card");
        Assert.NotNull(card);
    }

    [Fact]
    public void Index_RendersSetupContainer()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert
        var container = cut.Find(".setup-container");
        Assert.NotNull(container);
    }

    [Fact]
    public void Index_PlayerNameInputs_HaveCorrectPlaceholders()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert
        var inputs = cut.FindAll("input.form-control");
        Assert.Equal("King", inputs[0].GetAttribute("placeholder"));
        Assert.Equal("Player 2", inputs[1].GetAttribute("placeholder"));
        Assert.Equal("Player 3", inputs[2].GetAttribute("placeholder"));
    }

    // Helper class for NavigationManager mock
    private class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager()
        {
            Initialize("https://localhost/", "https://localhost/");
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            // Do nothing in tests
        }
    }
}
