# PoCoupleQuiz - AI Coding Agent Instructions

## Architecture Overview
This is a **.NET 10 Aspire** application with Blazor WebAssembly frontend and ASP.NET Core API backend. The solution uses **Vertical Slice Architecture** where features are organized by business capability, not technical layer.

### Project Structure
- **PoCoupleQuiz.AppHost** - Aspire orchestration (defines service dependencies, runs Azurite locally)
- **PoCoupleQuiz.Server** - API controllers + hosts Blazor WASM client
- **PoCoupleQuiz.Client** - Blazor WebAssembly UI (communicates via HTTP to Server)
- **PoCoupleQuiz.Core** - Shared models, services, validators (no UI/HTTP dependencies)
- **PoCoupleQuiz.ServiceDefaults** - Shared Aspire configuration (OpenTelemetry, health checks, resilience)

### Data Flow
Client (Blazor WASM) → Server (ASP.NET API) → Azure Table Storage (or Azurite locally)

## Key Patterns

### Service Registration
All services are registered in `PoCoupleQuiz.Core/Extensions/ServiceCollectionExtensions.cs`. Follow this pattern:
```csharp
services.AddSingleton<ITeamService, AzureTableTeamService>();
services.AddScoped<IGameStateService, GameStateService>();
```

### Client-Server Communication
Client services in `PoCoupleQuiz.Client/Services/` wrap HTTP calls to server APIs. Example: `HttpQuestionService` calls `/api/questions/*` endpoints.

### Azure OpenAI Integration
`IQuestionService` has two implementations:
- `AzureOpenAIQuestionService` - Production (requires Key Vault secrets)
- `MockQuestionService` - Local development fallback (used when `AzureOpenAI:ApiKey` is empty)

### Logging
Use **Serilog** with structured logging. Include context with properties:
```csharp
_logger.LogInformation("Game initialized - Players: {PlayerCount}", game.Players.Count);
```

## Development Commands

```powershell
# Start locally (launches Azurite container + all services via Aspire)
dotnet run --project PoCoupleQuiz.AppHost

# Run tests
dotnet test PoCoupleQuiz.Tests

# Run E2E tests (requires app running)
cd e2e-tests && npx playwright test

# Deploy to Azure Container Apps
azd auth login && azd up
```

## Testing Conventions
- **Unit tests**: `PoCoupleQuiz.Tests/UnitTests/` - Use `[Trait("Category", "Unit")]`
- **Integration tests**: `PoCoupleQuiz.Tests/IntegrationTests/` - Use `[Trait("Category", "Integration")]`
- **E2E tests**: `e2e-tests/` - Playwright tests in TypeScript
- Use **Moq** for mocking, **xUnit** for assertions

## Package Management
Uses **Central Package Management** via `Directory.Packages.props`. Add new packages there, not in individual `.csproj` files.

## Key Files to Reference
- [docs/adr/](docs/adr/) - Architecture Decision Records explain the "why" behind choices
- [PoCoupleQuiz.Core/Models/Game.cs](PoCoupleQuiz.Core/Models/Game.cs) - Core game domain model
- [PoCoupleQuiz.Core/Services/](PoCoupleQuiz.Core/Services/) - Business logic implementations
- [infra/main.bicep](infra/main.bicep) - Azure infrastructure definitions

## Game Domain Concepts
- **King Player**: One player answers questions about themselves; others guess
- **Difficulty**: Easy (3 rounds), Medium (5 rounds), Hard (7 rounds)
- **Scoring**: Only guessing players earn points for correct matches
