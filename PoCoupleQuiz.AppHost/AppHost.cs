using Aspire.Hosting.Azure;

var builder = DistributedApplication.CreateBuilder(args);

// ============================================================================
// AZURE STORAGE
// ============================================================================
// For local development with Key Vault: Set USE_AZURE_STORAGE=true
//   - Server loads connection string from Key Vault (PoCoupleQuiz--AzureStorage--ConnectionString)
//   - No Aspire storage resource needed
// Default local development: Use Azurite emulator (Docker container)
// For production (azd up): Aspire provisions an Azure Storage Account

var useAzureStorage = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("USE_AZURE_STORAGE"));

// ============================================================================
// SERVER PROJECT
// ============================================================================
// The Server project hosts both the API and the Blazor WASM client
// Configuration is loaded from Key Vault (kv-poshared) at runtime

var server = builder.AddProject<Projects.PoCoupleQuiz_Server>("server")
    .WithExternalHttpEndpoints();

if (useAzureStorage)
{
    // Using Azure Storage from Key Vault - server handles connection directly
    // Pass env var so server knows to use Key Vault connection string
    server = server.WithEnvironment("USE_AZURE_STORAGE", "true");
}
else if (builder.ExecutionContext.IsPublishMode)
{
    // Production deployment: Aspire provisions Azure Storage
    var storage = builder.AddAzureStorage("storage");
    var tables = storage.AddTables("tables");
    server = server.WithReference(tables).WaitFor(storage);
}
else
{
    // Local development default: Use Azurite emulator (Docker container)
    var storage = builder.AddAzureStorage("storage")
        .RunAsEmulator(emulator => emulator
            .WithLifetime(ContainerLifetime.Persistent));
    var tables = storage.AddTables("tables");
    server = server
        .WithReference(tables)
        .WaitFor(storage)
        .WithEnvironment("SKIP_KEYVAULT", "true");
}

builder.Build().Run();
