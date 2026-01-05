var builder = DistributedApplication.CreateBuilder(args);

// ============================================================================
// AZURE STORAGE (Azurite locally, Azure Storage in production)
// ============================================================================
// RunAsEmulator() uses the Azurite Docker image for local development
// In production (azd up), this provisions an Azure Storage Account
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(emulator => emulator
        .WithLifetime(ContainerLifetime.Persistent));

// Add Table Storage for game history and teams
var tables = storage.AddTables("tables");

// ============================================================================
// AZURE OPENAI CONFIGURATION
// ============================================================================
// Use shared Azure OpenAI Foundry service from PoShared resource group
var openAiEndpoint = builder.AddParameter("openAiEndpoint", secret: false);
var openAiKey = builder.AddParameter("openAiKey", secret: true);
var openAiDeployment = builder.AddParameter("openAiDeployment", secret: false);

// ============================================================================
// SERVER PROJECT
// ============================================================================
// The Server project hosts both the API and the Blazor WASM client
var server = builder.AddProject<Projects.PoCoupleQuiz_Server>("server")
    .WithExternalHttpEndpoints()
    .WithReference(tables)
    .WaitFor(storage)
    .WithEnvironment("AzureOpenAI__Endpoint", openAiEndpoint)
    .WithEnvironment("AzureOpenAI__Key", openAiKey)
    .WithEnvironment("AzureOpenAI__DeploymentName", openAiDeployment);

builder.Build().Run();
