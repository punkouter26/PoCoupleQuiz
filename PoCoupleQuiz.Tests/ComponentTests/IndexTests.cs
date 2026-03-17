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
    public void Index_RendersActionTabs()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert - two action tabs for Host and Join
        var tabs = cut.FindAll(".action-tab");
        Assert.Equal(2, tabs.Count);
        Assert.Contains(tabs, t => t.TextContent.Contains("Host Lobby"));
        Assert.Contains(tabs, t => t.TextContent.Contains("Join Lobby"));
    }

    [Fact]
    public void Index_RendersAssignedPlayerName()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert - player name is displayed
        var nameDisplay = cut.Find(".identity-value");
        Assert.NotNull(nameDisplay);
        Assert.NotEmpty(nameDisplay.TextContent);
    }

    [Fact]
    public void Index_DefaultTabIsHostLobby()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert - first tab is active by default
        var activeTabs = cut.FindAll(".action-tab.active");
        Assert.Single(activeTabs);
        Assert.Contains("Host Lobby", activeTabs[0].TextContent);
    }

    [Fact]
    public void Index_JoinTabShowsGameCodeInput()
    {
        // Act
        var cut = Render<IndexPage>();

        // Act - click on Join Lobby tab
        var joinTab = cut.FindAll(".action-tab")[1];
        joinTab.Click();

        // Assert - game code input appears
        var codeInput = cut.FindAll("input");
        Assert.Contains(codeInput, i => {
            var placeholder = i.GetAttribute("placeholder");
            return placeholder is not null && placeholder.Contains("XXXX");
        });
    }

    [Fact]
    public void Index_EnterLobbyButton_Exists()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert
        var button = cut.Find("button.btn-primary");
        Assert.NotNull(button);
        Assert.Contains(button.TextContent, new[] { "Enter Host Lobby", "Join Lobby" });
    }

    [Fact]
    public void Index_EnterLobbyButton_IsNotDisabledByDefault()
    {
        // Act
        var cut = Render<IndexPage>();

        // Assert - button is not disabled when not loading
        var button = cut.Find("button.btn-primary");
        Assert.False(button.HasAttribute("disabled") && button.GetAttribute("disabled") != "false",
            "Enter Lobby button should not be disabled initially");
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
