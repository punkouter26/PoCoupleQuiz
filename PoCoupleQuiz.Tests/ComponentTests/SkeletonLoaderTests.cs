using Bunit;
using Xunit;
using PoCoupleQuiz.Client.Shared;

namespace PoCoupleQuiz.Tests.ComponentTests;

/// <summary>
/// bUnit tests for SkeletonLoader component
/// </summary>
public class SkeletonLoaderTests : BunitContext
{
    [Fact]
    public void SkeletonLoader_QuestionType_RendersCorrectElements()
    {
        // Act
        var cut = Render<SkeletonLoader>(parameters => parameters
            .Add(p => p.Type, "question"));

        // Assert
        Assert.NotNull(cut.Find(".skeleton-title"));
        Assert.NotNull(cut.Find(".skeleton-subtitle"));
        Assert.NotNull(cut.Find(".skeleton-textarea"));
        Assert.NotNull(cut.Find(".skeleton-button"));
    }

    [Fact]
    public void SkeletonLoader_LeaderboardType_RendersHeaderAndFiveRows()
    {
        // Act
        var cut = Render<SkeletonLoader>(parameters => parameters
            .Add(p => p.Type, "leaderboard"));

        // Assert
        Assert.NotNull(cut.Find(".skeleton-table-header"));
        var rows = cut.FindAll(".skeleton-table-row");
        Assert.Equal(5, rows.Count);
    }

    [Fact]
    public void SkeletonLoader_DefaultType_RendersQuestionSkeleton()
    {
        // Act (no Type parameter, should default to "question")
        var cut = Render<SkeletonLoader>();

        // Assert
        Assert.NotNull(cut.Find(".skeleton-title"));
        Assert.NotNull(cut.Find(".skeleton-subtitle"));
        Assert.NotNull(cut.Find(".skeleton-textarea"));
        Assert.NotNull(cut.Find(".skeleton-button"));
    }

    [Fact]
    public void SkeletonLoader_AllTypes_RenderSkeletonContainer()
    {
        // Arrange
        var types = new[] { "question", "leaderboard" };

        foreach (var type in types)
        {
            // Act
            var cut = Render<SkeletonLoader>(parameters => parameters
                .Add(p => p.Type, type));

            // Assert
            Assert.NotNull(cut.Find(".skeleton-container"));
        }
    }

    [Fact]
    public void SkeletonLoader_InvalidType_DoesNotRenderContent()
    {
        // Act
        var cut = Render<SkeletonLoader>(parameters => parameters
            .Add(p => p.Type, "invalid-type"));

        // Assert
        var container = cut.Find(".skeleton-container");
        Assert.NotNull(container);
        // Should have no child skeleton elements for invalid type
        var skeletonElements = cut.FindAll(".skeleton");
        Assert.Empty(skeletonElements);
    }
}
