using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Components;
using PoCoupleQuiz.Core.Models;
using PoCoupleQuiz.Client.Services;
using IndexPage = PoCoupleQuiz.Client.Pages.Index;

namespace PoCoupleQuiz.Tests.ComponentTests;

/// <summary>
/// bUnit tests for Index (Home) page component.
/// Updated to match the redesigned Index.razor that uses SignalR hub for lobby creation.
/// </summary>
public class IndexTests : BunitContext
{
    private Mock<IGameHubService> _mockHubService = null!;

    public IndexTests()
    {
        SetupMocks();
    }

    private void SetupMocks()
    {
        _mockHubService = new Mock<IGameHubService>();

        // Setup hub to not throw on ConnectAsync (fire-and-forget in OnInitializedAsync)
        _mockHubService.Setup(h => h.ConnectAsync()).Returns(Task.CompletedTask);
        _mockHubService.Setup(h => h.DisposeAsync()).Returns(ValueTask.CompletedTask);

        // Wire up event stubs so += / -= don't throw
        _mockHubService.SetupAdd(h => h.OnLobbyCreated += It.IsAny<Action<LobbyInfo>>());
        _mockHubService.SetupAdd(h => h.OnLobbyJoined += It.IsAny<Action<LobbyInfo>>());
        _mockHubService.SetupAdd(h => h.OnLobbyError += It.IsAny<Action<string>>());
        _mockHubService.SetupRemove(h => h.OnLobbyCreated -= It.IsAny<Action<LobbyInfo>>());
        _mockHubService.SetupRemove(h => h.OnLobbyJoined -= It.IsAny<Action<LobbyInfo>>());
        _mockHubService.SetupRemove(h => h.OnLobbyError -= It.IsAny<Action<string>>());

        var navManager = new TestNavigationManager();

        Services.AddSingleton(_mockHubService.Object);
        Services.AddSingleton<NavigationManager>(navManager);
        Services.AddLogging();
    }

    [Fact]
    public void Index_ShouldRenderCorrectly()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert - the new design renders a home-wrapper and home-card
        var wrapper = cut.Find(".home-wrapper");
        Assert.NotNull(wrapper);
    }

    [Fact]
    public void Index_RendersAppTitle()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert - h1 with app title
        var title = cut.Find("h1.home-title");
        Assert.Contains("PoCoupleQuiz", title.TextContent);
    }

    [Fact]
    public void Index_RendersSubtitle()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert
        var subtitle = cut.Find("p.home-subtitle");
        Assert.Contains("Play together", subtitle.TextContent);
    }

    [Fact]
    public void Index_RendersNameInput()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert - single name input
        var inputs = cut.FindAll("input.form-control");
        Assert.Single(inputs);
        Assert.Equal("Enter your name", inputs[0].GetAttribute("placeholder"));
    }

    [Fact]
    public void Index_RendersDifficultyOptions()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert - three difficulty option cards
        var options = cut.FindAll(".difficulty-option");
        Assert.Equal(3, options.Count);
    }

    [Fact]
    public void Index_DefaultDifficulty_IsMedium()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert - Medium option is selected by default
        var selectedOptions = cut.FindAll(".difficulty-option.selected");
        Assert.Single(selectedOptions);
        Assert.Contains("Medium", selectedOptions[0].TextContent);
    }

    [Fact]
    public void Index_DifficultyOptions_HaveCorrectLabels()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert
        var options = cut.FindAll(".difficulty-option");
        Assert.Contains(options, o => o.TextContent.Contains("Easy"));
        Assert.Contains(options, o => o.TextContent.Contains("Medium"));
        Assert.Contains(options, o => o.TextContent.Contains("Hard"));
    }

    [Fact]
    public void Index_PlayButton_Exists()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert
        var button = cut.Find("button.btn-primary");
        Assert.Contains("Play", button.TextContent);
    }

    [Fact]
    public void Index_PlayButton_IsNotDisabledByDefault()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert - button is not disabled when not loading
        var button = cut.Find("button.btn-primary");
        Assert.False(button.HasAttribute("disabled") && button.GetAttribute("disabled") != "false",
            "Play button should not be disabled initially");
    }

    [Fact]
    public void Index_NoErrorMessage_ByDefault()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert - no error alert shown initially
        var errors = cut.FindAll(".alert-error");
        Assert.Empty(errors);
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
