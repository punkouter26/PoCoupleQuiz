using Xunit;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using PoCoupleQuiz.Tests.Utilities;

namespace PoCoupleQuiz.Tests;

/// <summary>
/// Integration tests for Health Check endpoints
/// </summary>
public class HealthCheckTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public HealthCheckTests()
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

    [Trait("Category", "Integration")]
    [Fact]
    [Trait("Category", "Integration")]
    public async Task ApiHealth_EndpointCalled_ReturnsHealthStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
        Assert.Contains("status", content.ToLower());
    }

    [Trait("Category", "Integration")]
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Health_RootEndpoint_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Trait("Category", "Integration")]
    [Fact]
    [Trait("Category", "Integration")]
    public async Task HealthLive_EndpointCalled_ReturnsLivenessStatus()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Trait("Category", "Integration")]
    [Fact]
    [Trait("Category", "Integration")]
    public async Task HealthReady_EndpointCalled_ReturnsReadinessStatus()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Trait("Category", "Integration")]
    [Fact]
    [Trait("Category", "Integration")]
    public async Task ApiHealth_ResponseParsed_ContainsRequiredChecks()
    {
        // Act
        var response = await _client.GetAsync("/api/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Verify the health check contains expected dependencies
        Assert.Contains("azure", content.ToLower());
    }
}