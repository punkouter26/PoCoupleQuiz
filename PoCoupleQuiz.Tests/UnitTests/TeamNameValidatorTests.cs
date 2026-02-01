using Xunit;
using PoCoupleQuiz.Core.Validators;

namespace PoCoupleQuiz.Tests.UnitTests;

/// <summary>
/// Consolidated tests for TeamNameValidator using Theory patterns.
/// Reduced from 9 individual tests to 3 parameterized tests.
/// </summary>
[Trait("Category", "Unit")]
public class TeamNameValidatorTests
{
    private readonly TeamNameValidator _validator;

    public TeamNameValidatorTests()
    {
        _validator = new TeamNameValidator();
    }

    [Theory]
    [InlineData("TeamAwesome", true, null)]
    [InlineData("Team Awesome", true, null)]
    [InlineData("AB", true, null)] // Min valid length
    [InlineData("A", false, "length")]
    [InlineData("", false, null)]
    [InlineData("   ", false, null)]
    public void Validate_VariousTeamNames_ReturnsExpectedResult(string? teamName, bool expectedValid, string? errorContains)
    {
        // Act
        var result = _validator.Validate(teamName ?? string.Empty);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
        if (!expectedValid)
        {
            Assert.NotNull(result.ErrorMessage);
            if (errorContains != null)
            {
                Assert.Contains(errorContains, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void Validate_NullTeamName_ReturnsError()
    {
        // Act
        var result = _validator.Validate(null!);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("required", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_TooLongTeamName_ReturnsError()
    {
        // Arrange - 51 chars exceeds 50 char limit
        var longName = new string('A', 51);

        // Act
        var result = _validator.Validate(longName);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("length", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }
}
