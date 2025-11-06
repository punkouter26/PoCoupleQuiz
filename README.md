# PoCoupleQuiz

An interactive web-based quiz application designed for couples and friends to test how well they know each other. Built with .NET 9, Blazor WebAssembly, and Azure services.

## What & Why

**PoCoupleQuiz** is a fun, engaging multiplayer quiz game where one player (the "King Player") answers questions about themselves, while other players try to guess their responses. The application demonstrates modern cloud-native development practices using vertical slice architecture, CQRS patterns, and comprehensive testing strategies.

**Why this project?**
- Showcase best practices for .NET 9 Blazor WebAssembly applications
- Demonstrate Azure service integration (Table Storage, OpenAI, Application Insights)
- Provide a reference implementation for clean architecture and TDD workflows
- Create an entertaining application for social gatherings

## Quick Start

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Azure Developer CLI (azd)](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd)
- [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) for local development
- Azure subscription (for Application Insights and Azure OpenAI)

### Local Development Setup

**For detailed setup instructions, see [LOCAL_DEVELOPMENT_SETUP.md](LOCAL_DEVELOPMENT_SETUP.md)**

**Quick reference: [QUICK_REFERENCE.md](QUICK_REFERENCE.md)**

#### 1. Provision Azure Infrastructure

```powershell
# Login to Azure
azd auth login

# Initialize and provision minimal Azure resources
azd init
azd provision
```

This creates only the essential cloud resources:
- Application Insights (for telemetry)
- Log Analytics Workspace (for Application Insights data)
- Uses shared Azure OpenAI resource

#### 2. Configure Local Secrets

```powershell
# Automated configuration (recommended)
.\infra\Configure-LocalSecrets.ps1

# Or manually configure (see LOCAL_DEVELOPMENT_SETUP.md)
```

#### 3. Start Azurite

Open a **new terminal window**:

```powershell
# Start Azure Storage emulator
azurite --silent --location c:\azurite --debug c:\azurite\debug.log
```

**Keep this terminal running** while developing.

#### 4. Run the Application

```powershell
# Navigate to server project
cd PoCoupleQuiz.Server

# Run the application
dotnet run
```

The application will start on a dynamic HTTPS port (look for the URL in console output).

#### 5. Verify Health Status

Navigate to: `https://localhost:[port]/diag`

You should see:
- ✅ API Service: Healthy
- ✅ Azure Table Storage: Healthy (connected to Azurite)
- ✅ Azure OpenAI: Healthy (connected to shared resource)

### How the App Works

The application consists of three main layers:

1. **Client (Blazor WebAssembly)**: Interactive UI that runs entirely in the browser
   - Responsive, mobile-first design
   - Real-time updates and animations
   - Client-side telemetry with Application Insights

2. **Server (.NET Web API)**: RESTful API with health checks and logging
   - Minimal APIs for lightweight endpoints
   - Serilog for structured logging
   - OpenTelemetry integration for observability

3. **Core (Business Logic)**: Shared domain models and services
   - Azure Table Storage for data persistence
   - Azure OpenAI for question generation
   - Polly for resilience policies

### Application Flow

1. Players register and select difficulty level
2. One player is designated as the "King Player"
3. Questions are generated (or selected from a predefined set)
4. King Player submits their answer
5. Other players submit their guesses
6. Scores are calculated based on correct matches
7. Results are displayed with a leaderboard

## Project Structure

### Source Projects (`/src`)

| Project | Description |
|---------|-------------|
| **PoCoupleQuiz.Client** | Blazor WebAssembly frontend with Radzen UI components |
| **PoCoupleQuiz.Server** | ASP.NET Core Web API backend with health checks and Swagger |
| **PoCoupleQuiz.Core** | Shared business logic, models, and Azure service integrations |

### Test Projects (`/tests`)

| Project | Description |
|---------|-------------|
| **PoCoupleQuiz.Tests** | xUnit tests (unit, integration, component tests with bUnit) |
| **e2e-tests** | Playwright end-to-end tests in TypeScript |

### Infrastructure (`/infra`)

Contains Bicep templates for Azure resource provisioning:
- `main.bicep`: Main infrastructure definition
- `resources.bicep`: Resource definitions
- `hooks/postprovision.ps1`: Post-deployment configuration

### Documentation (`/docs`)

- `PRD.md`: Product Requirements Document
- `adr/`: Architecture Decision Records
- `diagrams/`: Mermaid diagrams (C4 models, sequence diagrams)

## Key Features

- ✅ Multi-player support with King Player mechanics
- ✅ Dynamic question generation using Azure OpenAI
- ✅ Difficulty levels (Easy: 3 rounds, Medium: 5 rounds, Hard: 7 rounds)
- ✅ Real-time scoring and leaderboards
- ✅ Game statistics and player analytics
- ✅ Comprehensive health monitoring at `/diag`
- ✅ Responsive design for mobile and desktop
- ✅ Azure deployment with CI/CD

## Development Commands

```powershell
# Build the solution
dotnet build

# Restore packages
dotnet restore

# Format code
dotnet format

# Check for outdated packages
dotnet list package --outdated
```

## Testing

PoCoupleQuiz follows a **comprehensive testing strategy** with 80% minimum coverage requirement.

### Quick Test Commands

```powershell
# Run all backend tests (Unit + Integration + Component)
.\Run-AllTests.ps1

# Run tests by category
.\Run-AllTests.ps1 -Category Unit           # Fast unit tests only
.\Run-AllTests.ps1 -Category Integration    # API integration tests
.\Run-AllTests.ps1 -Category Component      # Blazor component tests
.\Run-AllTests.ps1 -Category E2E            # Playwright E2E tests

# Run all tests including E2E
.\Run-AllTests.ps1 -Category All

# Generate code coverage report (80% minimum)
.\Generate-CoverageReport.ps1
Start-Process docs\coverage\html\index.html
```

### Test Categories

| Category | Tests | Description |
|----------|-------|-------------|
| **Unit** | 23 | Isolated business logic (mocked dependencies) |
| **Integration** | 43 | End-to-end API tests (with Azurite) |
| **Component** | 4 | Blazor component rendering tests (bUnit) |
| **E2E** | TBD | Browser automation (Playwright + Chromium mobile) |

**Total Backend Tests**: 70 tests (98.6% pass rate)

### Test Requirements

✅ All tests categorized with `[Trait("Category", "...")]`  
✅ Naming convention: `MethodName_StateUnderTest_ExpectedBehavior`  
✅ 80% line coverage enforced  
✅ Local-only execution (no CI/CD costs)  

**For detailed testing guidelines, see [TESTING_GUIDELINES.md](docs/TESTING_GUIDELINES.md)**

## Deployment

### Automated CI/CD (Recommended)

**Phase 4** introduced a complete CI/CD pipeline with quality gates and security scanning.

**Deployment Flow**:
```
Push to main → Quality Gates (5 min) → Azure Deployment (10 min)
```

**Quality Gates**:
- ✅ CodeQL static security analysis
- ✅ Code formatting verification (`dotnet format`)
- ✅ Build validation
- ✅ Code coverage check (80% minimum)

**Authentication**: OIDC (OpenID Connect) - No secrets stored in GitHub

**First-Time Setup**:
1. Configure Azure AD federated credentials: [docs/OIDC_SETUP.md](docs/OIDC_SETUP.md)
2. Set GitHub repository variables (5 required)
3. Push to `main` branch or trigger manually

**Workflow File**: [.github/workflows/azure-dev.yml](.github/workflows/azure-dev.yml)

### Manual Deployment

Deploy to Azure using Azure Developer CLI:

```powershell
# Login to Azure
azd auth login

# Provision infrastructure + deploy application
azd up
```

**Resources Created**:
- App Service Plan (B1 Linux)
- App Service (.NET 9 runtime)
- Storage Account (Standard_LRS)
- Application Insights
- Log Analytics Workspace

**Estimated Cost**: ~$21 USD/month (low traffic)

See [docs/PHASE4_SUMMARY.md](docs/PHASE4_SUMMARY.md) for complete deployment documentation.

## Architecture Principles

- **Vertical Slice Architecture**: Features organized by business capability
- **CQRS**: Command/Query separation (future: MediatR integration)
- **Test-Driven Development**: Red-Green-Refactor workflow
- **Clean Code**: SOLID principles, documented design patterns
- **Observability**: OpenTelemetry tracing, custom business metrics, distributed correlation
- **Production Debugging**: Application Insights Snapshot Debugger and Profiler support

## Advanced Telemetry & Observability

**Phase 5** introduced world-class observability with OpenTelemetry instrumentation:

### End-to-End Distributed Tracing
- **W3C Trace Context**: Automatic correlation between Blazor client and .NET API
- **Custom ActivitySource**: Business operation tracing (game creation, scoring, AI generation)
- **operation_Id Propagation**: Complete user journey tracking from browser to database

### Custom Business Metrics
- **Game Metrics**: Duration, completion rate by difficulty
- **Player Metrics**: Active player count (real-time gauge), answer times
- **Performance Metrics**: AI question generation latency, storage operation performance
- **All metrics exported to Application Insights**: Query with KQL, visualize in dashboards

### Production Debugging Tools
- **Snapshot Debugger**: Capture memory dumps when exceptions occur (configured, disabled by default)
- **Profiler**: Method-level CPU profiling for performance optimization (configured, disabled by default)
- **Telemetry Enrichment**: Custom properties added to all telemetry (game ID, player role, difficulty)

### KQL Query Library
10 essential queries in `docs/kql/`:
- User activity and engagement metrics
- Server performance (P50/P95/P99 latency)
- Error rate monitoring
- Client-side exception tracking
- End-to-end trace correlation
- Custom metric visualization (timecharts)

**For complete telemetry documentation, see [docs/PHASE5_SUMMARY.md](docs/PHASE5_SUMMARY.md)**

## Contributing

Please follow the coding standards defined in `agents.md` and ensure all tests pass before submitting pull requests.

## License

This project is for demonstration purposes.

---

**Built with** ❤️ **using .NET 9, Blazor, and Azure**
