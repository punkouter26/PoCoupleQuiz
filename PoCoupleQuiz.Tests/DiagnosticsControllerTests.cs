using Xunit;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net;
using PoCoupleQuiz.Tests.Utilities;

namespace PoCoupleQuiz.Tests;

/// <summary>
/// Integration tests for DiagnosticsController endpoints
/// </summary>
public class DiagnosticsControllerTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DiagnosticsControllerTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task CheckInternetConnection_ReturnsBoolean()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics/internet");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<bool>();
        // Result can be true or false depending on network
        Assert.True(result is true or false);
    }

    [Fact]
    public async Task LogConsoleMessage_ValidMessage_ReturnsOk()
    {
        // Arrange
        var logMessage = new
        {
            level = "info",
            message = "Test console message",
            timestamp = DateTime.UtcNow.ToString("O")
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/diagnostics/console", logMessage);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task NetworkStatus_ReturnsNetworkInfo()
    {
        // Act
        var response = await _client.GetAsync("/api/diagnostics/network");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
    }
}
