using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PoCoupleQuiz.Core.Services;

namespace PoCoupleQuiz.Tests.UnitTests;

[Trait("Category", "Unit")]
public class ConsoleLoggingServiceTests
{
    private readonly Mock<ILogger<ConsoleLoggingService>> _mockLogger;
    private readonly ConsoleLoggingService _service;

    public ConsoleLoggingServiceTests()
    {
        _mockLogger = new Mock<ILogger<ConsoleLoggingService>>();
        _service = new ConsoleLoggingService(_mockLogger.Object);
    }

    [Theory]
    [InlineData("info", "Test message")]
    [InlineData("information", "Test message")]
    public void LogMessage_InfoLevel_LogsInformation(string level, string message)
    {
        // Act
        _service.LogMessage(level, message);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("warn", "Warning message")]
    [InlineData("warning", "Warning message")]
    public void LogMessage_WarnLevel_LogsWarning(string level, string message)
    {
        // Act
        _service.LogMessage(level, message);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogMessage_ErrorLevel_LogsError()
    {
        // Arrange
        var message = "Error message";

        // Act
        _service.LogMessage("error", message);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogMessage_DebugLevel_LogsDebug()
    {
        // Arrange
        var message = "Debug message";

        // Act
        _service.LogMessage("debug", message);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogMessage_WithCategory_IncludesCategoryInMessage()
    {
        // Arrange
        var message = "Test message";
        var category = "TestCategory";

        // Act
        _service.LogMessage("info", message, category);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(category) && v.ToString().Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogMessage_UnknownLevel_DefaultsToInformation()
    {
        // Arrange
        var message = "Test message";

        // Act
        _service.LogMessage("unknown", message);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
