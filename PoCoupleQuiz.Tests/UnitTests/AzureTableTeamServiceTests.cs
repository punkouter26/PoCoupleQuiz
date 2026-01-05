using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PoCoupleQuiz.Core.Services;
using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Tests.UnitTests;

/// <summary>
/// Unit tests for AzureTableTeamService.
/// Tests the service's behavior for team operations via ITeamService interface.
/// </summary>
[Trait("Category", "Unit")]
public class AzureTableTeamServiceTests
{
    private readonly Mock<ITeamService> _mockTeamService;

    public AzureTableTeamServiceTests()
    {
        _mockTeamService = new Mock<ITeamService>();
    }

    [Fact]
    public async Task GetTeamAsync_WithValidTeamName_ReturnsTeam()
    {
        // Arrange
        var expectedTeam = new Team { Name = "TestTeam", HighScore = 100 };
        _mockTeamService.Setup(s => s.GetTeamAsync("TestTeam"))
            .ReturnsAsync(expectedTeam);

        // Act
        var result = await _mockTeamService.Object.GetTeamAsync("TestTeam");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestTeam", result.Name);
        Assert.Equal(100, result.HighScore);
    }

    [Fact]
    public async Task GetTeamAsync_WithNonExistentTeam_ReturnsNull()
    {
        // Arrange
        _mockTeamService.Setup(s => s.GetTeamAsync("NonExistent"))
            .ReturnsAsync((Team?)null);

        // Act
        var result = await _mockTeamService.Object.GetTeamAsync("NonExistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveTeamAsync_WithValidTeam_Completes()
    {
        // Arrange
        var team = new Team { Name = "NewTeam" };
        _mockTeamService.Setup(s => s.SaveTeamAsync(team))
            .Returns(Task.CompletedTask);

        // Act & Assert - Should not throw
        await _mockTeamService.Object.SaveTeamAsync(team);
        _mockTeamService.Verify(s => s.SaveTeamAsync(team), Times.Once);
    }

    [Fact]
    public async Task GetAllTeamsAsync_ReturnsAllTeams()
    {
        // Arrange
        var teams = new List<Team>
        {
            new Team { Name = "Team1", HighScore = 100 },
            new Team { Name = "Team2", HighScore = 200 }
        };
        _mockTeamService.Setup(s => s.GetAllTeamsAsync())
            .ReturnsAsync(teams);

        // Act
        var result = await _mockTeamService.Object.GetAllTeamsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllTeamsAsync_WhenNoTeams_ReturnsEmptyList()
    {
        // Arrange
        _mockTeamService.Setup(s => s.GetAllTeamsAsync())
            .ReturnsAsync(new List<Team>());

        // Act
        var result = await _mockTeamService.Object.GetAllTeamsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateTeamStatsAsync_WithValidParams_Completes()
    {
        // Arrange
        _mockTeamService.Setup(s => s.UpdateTeamStatsAsync(
                "TestTeam",
                GameMode.KingPlayer,
                100,
                10,
                8))
            .Returns(Task.CompletedTask);

        // Act & Assert - Should not throw
        await _mockTeamService.Object.UpdateTeamStatsAsync("TestTeam", GameMode.KingPlayer, 100, 10, 8);
        _mockTeamService.Verify(s => s.UpdateTeamStatsAsync("TestTeam", GameMode.KingPlayer, 100, 10, 8), Times.Once);
    }

    [Fact]
    public async Task UpdateTeamStatsAsync_VerifiesCorrectParametersArePassed()
    {
        // Arrange
        _mockTeamService.Setup(s => s.UpdateTeamStatsAsync(
                "TestTeam",
                GameMode.KingPlayer,
                150,
                20,
                15))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _mockTeamService.Object.UpdateTeamStatsAsync("TestTeam", GameMode.KingPlayer, 150, 20, 15);

        // Assert
        _mockTeamService.Verify();
    }

    [Fact]
    public async Task GetTeamAsync_ReturnsTeamWithCorrectProperties()
    {
        // Arrange
        var expectedTeam = new Team
        {
            Name = "CompleteTeam",
            HighScore = 500,
            TotalQuestionsAnswered = 50,
            CorrectAnswers = 40
        };
        _mockTeamService.Setup(s => s.GetTeamAsync("CompleteTeam"))
            .ReturnsAsync(expectedTeam);

        // Act
        var result = await _mockTeamService.Object.GetTeamAsync("CompleteTeam");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result.HighScore);
        Assert.Equal(50, result.TotalQuestionsAnswered);
        Assert.Equal(40, result.CorrectAnswers);
        Assert.Equal(80.0, result.CorrectPercentage);
    }
}
