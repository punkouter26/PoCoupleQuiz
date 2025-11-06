1. Core Architecture & Principles
Architecture Style: All code must be organized using Vertical Slice Architecture. Group files by feature (e.g., /Features/GetProduct/Endpoint.cs), not by technical layer.
Design Philosophy: Prioritize simple, well-factored, and self-contained code. Apply SOLID principles pragmatically. If a Gang of Four (GoF) design pattern is used, document its use.
Tooling:
Use MediatR to implement the CQRS pattern within vertical slices.
Use Minimal APIs for all new API endpoints.
Utilize OpenTelemetry abstractions (ActivitySource for traces, Meter for metrics) for custom application telemetry.
Consider tools like Polly for resilience and dotnet-monitor for on-demand diagnostics where they add clear value.
2. Solution & Project Structure
Root Folders: Adhere to the standard folder structure: /src, /tests, /docs, /infra, and /scripts.
Naming Conventions:
All projects, solutions, and Azure Storage tables must use the prefix Po.[AppName].*.
The user-facing HTML &lt;title&gt; tag must include the Po. prefix.
Project Organization:
/src/Po.[AppName].Api/Features/: Contains backend vertical slices.
/src/Po.[AppName].Client/: Contains the Blazor frontend application.
/src/Po.[AppName].Shared/: Contains DTOs and shared models.
Dependency Management: All NuGet packages must be managed centrally in a single Directory.Packages.props file at the repository root.
3. Environment & Configuration
.NET SDK: All projects must target .NET 9. The global.json file must be pinned to a specific 9.0.xxx SDK version.
Local Secrets: All sensitive keys (connection strings, API keys) for local development must be stored using the .NET User Secrets manager. They must not be committed to appsettings.json.
Storage: Default to Azure Table Storage. For local development and integration testing, use Azurite.
Null Safety: Nullable Reference Types (&lt;Nullable&gt;enable&lt;/Nullable&gt;) must be enabled in all .csproj files.
CLI Usage: Prefer single-line CLI commands over creating one-shot PowerShell scripts for simple tasks.
4. Backend (API) Implementation
API Documentation:
Enable Swagger (OpenAPI) generation for all API endpoints.
Generate .http files with sample requests for all endpoints to facilitate easy manual testing.
Health Checks: Implement mandatory liveness and readiness health check endpoints:
/api/health/live (Liveness)
/api/health/ready (Readiness), which must include checks for all critical external dependencies.
Logging & Telemetry:
Logging: Use Serilog for structured logging. Configure it via appsettings.json to write to the Debug Console in Development and Application Insights in Production.
Enrichment: Enrich all logs with a CorrelationId using Activity.Current.Id.
Error Handling: Use structured ILogger.LogWarning or LogError within all catch blocks.
5. Frontend (Client) Implementation
UI Framework: Use Microsoft.FluentUI.AspNetCore.Components as the primary component library. The Radzen.Blazor library should only be used if its tools are essential for a specific, complex requirement.
Responsive Design: The UI must be mobile-first (portrait mode), responsive, fluid, and touch-friendly, ensuring a professional layout on desktop views.
Client-Side Telemetry: The Blazor client must integrate the Application Insights JavaScript SDK to capture page views, client-side errors, and performance metrics.


6. Testing Strategy
Workflow: Strictly follow a Test-Driven Development (TDD) workflow: Red -&gt; Green -&gt; Refactor.
Test Naming: Test methods must follow the MethodName_StateUnderTest_ExpectedBehavior convention.
Unit Tests (xUnit):
Must cover all new backend business logic (e.g., MediatR handlers).
All external dependencies must be mocked.
Component Tests (bUnit):
Must cover all new Blazor components, including rendering, user interactions, and state changes.
Mock dependencies such as IHttpClientFactory and IJSRuntime.
Integration Tests (xUnit):
Create a "happy path" test for every new API endpoint using WebApplicationFactory.
Tests must run against an isolated, in-memory Azurite instance. No data shall persist between test runs.
Use the Bogus library to generate realistic test data.
E2E Tests (Playwright):
Write tests in TypeScript, targeting Chromium for both mobile and desktop views.
Use network interception (page.route()) to mock API responses for stable testing.
Integrate axe-core for automated accessibility checks.
Implement screenshot testing for visual regression checks.
7. Infrastructure as Code (Bicep)
Automation: All Azure resources must be defined in Bicep files and be fully deployable via the azd up command without requiring manual user input.
Security:
Enable System-Assigned Managed Identity on the Azure App Service.
Provision an Azure Key Vault and grant the App Service's Managed Identity Get/List secret permissions.
Configure the App Service to use Key Vault References to access secrets. Do not check secrets into any configuration files.
Observability: Provision Microsoft.Insights/diagnosticSettings for all resources, configured to forward all logs and metrics to a central Log Analytics Workspace.
