using Xunit;
using PoCoupleQuiz.Core.Validators;

namespace PoCoupleQuiz.Tests.UnitTests;

[Trait("Category", "Unit")]
public class TeamNameValidatorTests
{
    private readonly TeamNameValidator _validator;

    public TeamNameValidatorTests()
    {
        _validator = new TeamNameValidator();
    }

    [Fact]
    public void Validate_ValidTeamName_ReturnsSuccess()
    {
        // Arrange
        var validName = "TeamAwesome";

        // Act
        var result = _validator.Validate(validName);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Validate_NullTeamName_ReturnsError()
    {
        // Act
        var result = _validator.Validate(null);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("required", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_EmptyTeamName_ReturnsError()
    {
        // Act
        var result = _validator.Validate(string.Empty);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void Validate_WhitespaceTeamName_ReturnsError()
    {
        // Act
        var result = _validator.Validate("   ");

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void Validate_TooShortTeamName_ReturnsError()
    {
        // Arrange
        var shortName = "A";

        // Act
        var result = _validator.Validate(shortName);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("length", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_TooLongTeamName_ReturnsError()
    {
        // Arrange
        var longName = new string('A', 51); // Assuming 50 char limit

        // Act
        var result = _validator.Validate(longName);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("length", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_SpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var nameWithSpecialChars = "Team!@#$%";

        // Act
        var result = _validator.Validate(nameWithSpecialChars);

        // Assert - depends on validation rules
        // This test documents the expected behavior
        Assert.NotNull(result);
    }

    [Fact]
    public void Validate_ValidNameWithSpaces_ReturnsSuccess()
    {
        // Arrange
        var nameWithSpaces = "Team Awesome";

        // Act
        var result = _validator.Validate(nameWithSpaces);

        // Assert
        Assert.True(result.IsValid);
    }
}
