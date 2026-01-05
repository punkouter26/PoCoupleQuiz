# Coding Agents Rules

This document contains the coding rules and standards for AI assistants and developers working on PoCoupleQuiz.

## 1. Project Identity & SDK Standards

- **Unified ID Strategy**: Use `PoCoupleQuiz` as the master identifier for Azure Resource Groups and ACA environments
- **Modern SDK**: Target **.NET 10** and **C# 14** exclusively
- **Central Package Management**: Enforce CPM via `Directory.Packages.props` with `CentralPackageTransitivePinning` enabled
- **AOT-First**: Enable `<IsAotCompatible>true</IsAotCompatible>` where possible

## 2. Orchestration & Inner-Loop (The Aspire Way)

- **Aspire Ecosystem**: Use `dotnet new aspire-starter`. The **AppHost** is the source of truth for local orchestration
- **Dynamic Endpoints**: Avoid hardcoded ports in launchSettings.json. Rely on Aspire's named references
- **Startup Sequencing**: Use `.WaitFor(resource)` for ordering and `.WithLifetime(ContainerLifetime.Persistent)` for infrastructure

## 3. Architecture: Flattened Vertical Slice (VSA)

- **Feature Folders**: Keep Endpoints, DTOs, and Business Logic together within a single feature folder
- **Result Pattern**: Use the `ErrorOr` library. Minimal APIs must use `.Match()` to return `TypedResults`
- **Data Access**: Use lightweight expression visitors to map DTO filters directly to `TableClient`

## 4. UI & Security (BFF Pattern)

- **Secure BFF**: The API acts as the security proxy for the Blazor WASM client
- **Cookie-Only Security**: The WASM client handles Secure Cookies only; never touches JWTs
- **State Management**: Use Component Parameters for parent-child flow, Scoped StateContainer for global state

## 5. Secret & Configuration Management

- **Zero-Trust Config**: Use Azure Key Vault Configuration Provider
- **Secrets at Runtime**: Secrets are fetched via Managed Identity, never stored in code

## 6. Resilience & Observability

- **Native Resilience**: Apply `.AddStandardResilienceHandler()` (Polly) to all HttpClient configurations
- **Source-Generated Logging**: Use `LoggerMessage` Delegates instead of Serilog where performance is critical
- **Health Probes**: Use standard `MapHealthChecks("/health")` with Readiness checks for backing services
- **Telemetry**: Enable OpenTelemetry for tracing and metrics, exported to Azure Monitor

## 7. Infrastructure & Deployment

- **Provisioning**: Use `azd up` to deploy the Aspire model to Azure Container Apps
- **CI/CD**: GitHub Actions workflow builds and deploys via azd
- **Minimal Infrastructure**: Let Aspire generate Bicep modules automatically

## Quick Commands

```powershell
# Local development
dotnet run --project PoCoupleQuiz.AppHost

# Run tests
dotnet test

# Deploy to Azure
azd up
```
