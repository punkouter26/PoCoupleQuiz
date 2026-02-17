# PoCoupleQuiz

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor WASM](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?logo=blazor)](https://blazor.net/)
[![Azure](https://img.shields.io/badge/Azure-App%20Service-0078D4?logo=microsoftazure)](https://azure.microsoft.com/services/app-service/)
[![License](https://img.shields.io/badge/License-Demo-gray)](LICENSE)

An interactive web-based quiz application designed for couples and friends to test how well they know each other. Built with **.NET 10**, **Blazor WebAssembly**, and **Azure App Service**.

## What & Why

**PoCoupleQuiz** is a fun, multiplayer quiz game where one player (the "King Player") answers questions about themselves, while other players try to guess their responses. 

**Why this project?**
- Showcase best practices for .NET 10 Blazor WebAssembly applications
- Demonstrate Azure service integration (Table Storage, OpenAI, Application Insights)
- Reference implementation for Vertical Slice Architecture
- Create an entertaining application for social gatherings

## Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Azure Developer CLI (azd)](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd)
- [Docker](https://www.docker.com/) (for Azurite local development)

### Local Development

```powershell
# 1. Start Azurite (Azure Storage emulator)
docker run -d --name azurite -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite

# 2. Run the application via Aspire
dotnet run --project PoCoupleQuiz.AppHost
```

Open the Aspire Dashboard URL shown in the console (e.g., `https://localhost:17011`).

### Run Tests

```powershell
# Run all unit and integration tests
dotnet test PoCoupleQuiz.Tests

# Run E2E tests (requires app running)
cd e2e-tests && npx playwright test
```

### Deploy to Azure

#### Option 1: GitHub Actions CI/CD (Recommended)

Push to `master` branch automatically triggers build and deployment to **Azure App Service**.

**Deployment Target**: `https://pocouplequiz-app.azurewebsites.net/`

**Required GitHub Repository Variables** (Settings → Secrets and variables → Actions → Variables):
| Variable | Value |
|----------|-------|
| `AZURE_CLIENT_ID` | Service Principal Client ID |
| `AZURE_TENANT_ID` | Your tenant ID |

**Deployment Process**:
1. Build .NET application (Release configuration)
2. Run unit tests
3. Publish to Azure App Service
4. Configure application settings from Key Vault
5. Health check verification

**Secrets Management**:  
Keys are stored in `kv-poshared` Azure Key Vault and referenced during deployment:
- `PoCoupleQuiz--ApplicationInsights--ConnectionString`
- `PoCoupleQuiz--AzureStorage--ConnectionString`
- `PoCoupleQuiz--AzureOpenAI--*` (if using AI features)

#### Option 2: Local Deployment

```powershell
# Build and publish
dotnet publish PoCoupleQuiz.Server/PoCoupleQuiz.Server.csproj -c Release -o ./publish

# Deploy to app service
az webapp deploy --resource-group PoCoupleQuiz --name pocouplequiz-app --src-path ./publish.zip --type zip
```

#### Option 3: Azure Developer CLI

```powershell
azd auth login
azd up
```

## Deployment Report

📊 **[Full Deployment Report](.azure/deployment-report.md)**

Key information:
- **App Service URL**: https://pocouplequiz-app.azurewebsites.net/
- **App Service Plan**: asp-poshared-linux (PoShared RG) - B1 tier Linux
- **Runtime**: DOTNETCORE 8.0
- **Status**: Deployed & Running
- **Monitoring**: Application Insights connected
- **Storage**: Azure Table Storage (stpocouplequizapp)

## How the Game Works

1. **Register**: Players enter team name and player names
2. **King Selection**: First player becomes the "King"
3. **Question Round**: King answers a question about themselves
4. **Guessing**: Other players try to guess the King's answer
5. **Scoring**: Points awarded for correct matches
6. **Rotate**: Next player becomes King
7. **Results**: Final leaderboard displayed

## Project Structure

| Project | Description |
|---------|-------------|
| `PoCoupleQuiz.AppHost` | Aspire orchestration (local development) |
| `PoCoupleQuiz.Client` | Blazor WebAssembly frontend |
| `PoCoupleQuiz.Server` | ASP.NET Core Web API backend |
| `PoCoupleQuiz.Core` | Shared business logic and models |
| `PoCoupleQuiz.ServiceDefaults` | Shared service configuration |
| `PoCoupleQuiz.Tests` | xUnit tests (unit, integration, component) |
| `e2e-tests` | Playwright E2E tests |

## Documentation

| Document | Description |
|----------|-------------|
| [Architecture Overview](docs/product/architecture-overview.md) | System design and patterns |
| [Developer Walkthrough](docs/product/walkthrough.md) | Getting started guide |
| [Features](docs/product/features.md) | Complete feature documentation |
| [PRD](docs/PRD.md) | Product Requirements Document |
| [ADRs](docs/adr/) | Architecture Decision Records |
| [Diagrams](docs/diagrams/) | Architecture diagrams (Mermaid + SVG) |
| [KQL Queries](docs/kql/) | Application Insights monitoring |
| [API Reference](docs/api/) | REST API documentation & .http files |
| [Key Vault Mapping](docs/mapping/keyvault-mapping.md) | Secret configuration reference |
| [Coding Standards](agents.md) | AI agent coding rules |

## API Documentation

When running locally, OpenAPI documentation is available at:
```
https://localhost:5001/openapi/v1.json
```

For quick API testing, use the [REST Client](docs/api/api-endpoints.http) with VS Code.

## Key Features

- ✅ Multi-player support with King Player mechanics
- ✅ Dynamic question generation using Azure OpenAI
- ✅ Difficulty levels (Easy: 3 rounds, Medium: 5 rounds, Hard: 7 rounds)
- ✅ Real-time scoring and leaderboards
- ✅ Responsive mobile-first design
- ✅ Azure Container Apps deployment via Aspire
- ✅ OpenTelemetry observability

## Architecture

- **Vertical Slice Architecture**: Features organized by business capability
- **Aspire Orchestration**: Local development with service discovery
- **BFF Pattern**: Server acts as security proxy for client
- **Result Pattern**: ErrorOr for explicit error handling

## Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| "Connection refused" on startup | Ensure Docker is running and Azurite container is active |
| "MockQuestionService being used" | Normal for local dev - Azure OpenAI secrets not configured |
| Tests skipped | Azure OpenAI integration tests require API keys |
| "Key Vault access denied" | Check managed identity has `Key Vault Secrets User` role |
| Blazor debugging not working | Use Edge/Chrome, ensure WASM debugging is enabled in launch settings |

### Logs & Diagnostics

```powershell
# View Aspire dashboard logs
# Open https://localhost:17011 after starting AppHost

# Check health endpoints
curl https://localhost:5001/health/ready
curl https://localhost:5001/api/health

# View Application Insights (production)
# Use KQL queries in docs/kql/
```

### Reset Local State

```powershell
# Stop and remove Azurite container
docker stop azurite && docker rm azurite

# Clear .NET build artifacts
dotnet clean && dotnet build
```

## Contributing

### Getting Started

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Follow coding standards in [agents.md](agents.md)
4. Ensure tests pass: `dotnet test`
5. Submit a pull request

### Code Quality Checklist

- [ ] Unit tests for new functionality
- [ ] No compiler warnings
- [ ] XML documentation for public APIs
- [ ] Follows Vertical Slice Architecture
- [ ] Uses structured logging with Serilog
- [ ] Validates inputs with FluentValidation patterns

### Commit Message Format

```
<type>: <description>

[optional body]
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

## License

This project is for demonstration purposes.

---

**Built with** ❤️ **using .NET 10, Blazor, Aspire, and Azure**
