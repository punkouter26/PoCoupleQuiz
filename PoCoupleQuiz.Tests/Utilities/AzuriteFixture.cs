using Testcontainers.Azurite;
using Xunit;

namespace PoCoupleQuiz.Tests.Utilities;

/// <summary>
/// Shared Azurite container fixture for integration tests.
/// Uses Testcontainers to spin up ephemeral Azurite instance.
/// </summary>
public class AzuriteFixture : IAsyncLifetime
{
    private readonly AzuriteContainer _container;
    
    public AzuriteFixture()
    {
        _container = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
            .Build();
    }

    public string ConnectionString => _container.GetConnectionString();
    
    public string TableEndpoint => $"http://{_container.Hostname}:{_container.GetMappedPublicPort(10002)}/devstoreaccount1";

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

/// <summary>
/// Collection definition for sharing Azurite container across integration tests.
/// </summary>
[CollectionDefinition("Azurite")]
public class AzuriteCollection : ICollectionFixture<AzuriteFixture>
{
}
