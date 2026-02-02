# Architecture Overview

## Introduction

PoCoupleQuiz is a modern web application built on **.NET 10** and **Blazor WebAssembly**, designed to demonstrate best practices for cloud-native applications with Azure integration.

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Client (Browser)                              │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │           Blazor WebAssembly (PoCoupleQuiz.Client)          │   │
│  │  ┌─────────┐  ┌──────────┐  ┌─────────────────────────┐    │   │
│  │  │  Pages  │  │ Services │  │ GameStateService (state)│    │   │
│  │  └─────────┘  └──────────┘  └─────────────────────────┘    │   │
│  └─────────────────────────────────────────────────────────────┘   │
└───────────────────────────────┬─────────────────────────────────────┘
                                │ HTTPS
┌───────────────────────────────▼─────────────────────────────────────┐
│                    ASP.NET Core Server (Host)                        │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │              PoCoupleQuiz.Server (BFF Pattern)              │   │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────────────┐    │   │
│  │  │Controllers │  │ Middleware │  │ OpenTelemetry      │    │   │
│  │  └────────────┘  └────────────┘  └────────────────────┘    │   │
│  └─────────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │              PoCoupleQuiz.Core (Business Logic)             │   │
│  │  ┌──────────┐  ┌───────────┐  ┌────────────────────────┐   │   │
│  │  │ Services │  │ Validators│  │ Models                 │   │   │
│  │  └──────────┘  └───────────┘  └────────────────────────┘   │   │
│  └─────────────────────────────────────────────────────────────┘   │
└───────────────────────────────┬─────────────────────────────────────┘
                                │
┌───────────────────────────────▼─────────────────────────────────────┐
│                         Azure Cloud                                  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────────┐  │
│  │Table Storage │  │ Azure OpenAI │  │ Application Insights     │  │
│  │ (Teams, Games)│  │ (GPT-4o)     │  │ (Telemetry)              │  │
│  └──────────────┘  └──────────────┘  └──────────────────────────┘  │
│  ┌──────────────┐  ┌──────────────────────────────────────────────┐│
│  │  Key Vault   │  │ Azure Container Apps (via Aspire)            ││
│  │  (Secrets)   │  │                                              ││
│  └──────────────┘  └──────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────────┘
```

## Architecture Patterns

### 1. Vertical Slice Architecture

Features are organized by **business capability**, not technical layer:

```
PoCoupleQuiz.Core/
├── Models/           # Domain models (Game, Team, Question)
├── Services/         # Business logic (GameStateService, TeamService)
├── Validators/       # Input validation (TeamNameValidator)
└── Extensions/       # DI registration (ServiceCollectionExtensions)
```

**Benefits:**
- High cohesion within each feature
- Low coupling between features
- Easy to add/remove features
- Clear ownership boundaries

### 2. Backend-for-Frontend (BFF) Pattern

The Server hosts the Blazor WASM client and acts as a secure proxy:

```
Client → Server (BFF) → Azure Services
```

**Benefits:**
- Server handles authentication/authorization
- Secrets never exposed to client
- Server can cache/transform responses
- Single deployment unit

### 3. Result Pattern with ErrorOr

Explicit error handling without exceptions for expected failures:

```csharp
public async Task<ErrorOr<Question>> GenerateQuestionAsync(string difficulty)
{
    if (string.IsNullOrEmpty(_apiKey))
        return Error.Failure("OpenAI.NotConfigured", "API key missing");
    
    // ... generate question
    return question;
}
```

## Project Structure

| Project | Purpose | Dependencies |
|---------|---------|--------------|
| `PoCoupleQuiz.AppHost` | Aspire orchestration | All projects |
| `PoCoupleQuiz.Client` | Blazor WASM UI | Core |
| `PoCoupleQuiz.Server` | API host + BFF | Core, Client |
| `PoCoupleQuiz.Core` | Business logic | None (leaf) |
| `PoCoupleQuiz.ServiceDefaults` | Shared config | Aspire packages |
| `PoCoupleQuiz.Tests` | Test suite | All projects |

## Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| Runtime | .NET | 10.0 |
| Frontend | Blazor WebAssembly | 10.0 |
| UI Components | Radzen Blazor | 8.7.4 |
| Backend | ASP.NET Core | 10.0 |
| Orchestration | .NET Aspire | 13.1.0 |
| Logging | Serilog | 5.0.1 |
| Telemetry | OpenTelemetry | 1.15.0 |
| Storage | Azure Table Storage | 12.10.0 |
| AI | Azure OpenAI | 2.2.0 |
| Testing | xUnit + Moq + Playwright | Latest |

## Data Flow

### Question Generation Flow

```
1. User clicks "Next Question" in Game.razor
2. Client calls HttpQuestionService.GenerateQuestionAsync()
3. HTTP POST to /api/questions/generate
4. QuestionsController validates request
5. AzureOpenAIQuestionService builds prompt
6. Azure OpenAI GPT-4o generates question
7. Response cached locally (in-memory)
8. Question returned to UI
9. OpenTelemetry logs latency metric
```

### Answer Submission Flow

```
1. Player submits answer in Game.razor
2. GameStateService.RecordAnswer() updates state
3. When all players answered:
   - HTTP POST to /api/questions/check-similarity
   - AI compares answers semantically
   - Scores calculated and returned
4. HTTP PUT to /api/teams/{name}/stats
5. Team statistics persisted to Table Storage
```

## Deployment Architecture

### Local Development (Aspire)

```powershell
dotnet run --project PoCoupleQuiz.AppHost
```

Aspire orchestrates:
- Azurite container (storage emulator)
- Server with hot reload
- Client WASM with debugging
- Dashboard at https://localhost:17011

### Production (Azure Container Apps)

```powershell
azd up
```

Deploys:
- Container App with managed identity
- Key Vault for secrets
- Table Storage account
- Application Insights workspace
- Container Registry for images

## Security Model

| Concern | Solution |
|---------|----------|
| Secrets | Azure Key Vault + Managed Identity |
| Input Validation | FluentValidation + TeamNameValidator |
| XSS Protection | Blazor auto-escaping |
| HTTPS | Enforced in production |
| API Security | BFF pattern (no direct Azure access from client) |

## Observability

| Signal | Technology | Destination |
|--------|------------|-------------|
| Logs | Serilog structured logging | Application Insights |
| Metrics | OpenTelemetry custom metrics | Application Insights |
| Traces | OpenTelemetry distributed tracing | Application Insights |
| Health | ASP.NET Health Checks | Kubernetes probes |

## Related Documentation

- [ADR-001: Serilog for Logging](../adr/001-use-serilog-for-structured-logging.md)
- [ADR-002: Azure Table Storage](../adr/002-use-azure-table-storage.md)
- [ADR-003: Key Vault References](../adr/003-use-key-vault-references-for-secrets.md)
- [ADR-004: Radzen Blazor](../adr/004-use-radzen-blazor-components.md)
- [ADR-005: Vertical Slice Architecture](../adr/005-use-vertical-slice-architecture.md)
- [ADR-006: .NET 10 Upgrade](../adr/006-upgrade-to-dotnet-10.md)
