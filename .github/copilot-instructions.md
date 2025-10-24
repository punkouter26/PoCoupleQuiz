Quick Reference
SDK: .NET 9.0 latest patch.
Ports: API must bind to HTTP 5000 and HTTPS 5001 only.
Project layout: /src, /tests, /docs, /scripts at repo root.
Project name prefix: Po.AppName.*.
Storage: Azure Table Storage by default using Azurite locally; other stores require spec.
Error handling: RFC 7807 Problem Details via Serilog; never expose raw exceptions.
Testing: xUnit for unit and integration; Playwright MCP TypeScript for E2E run manually.
UI focus: Excellent mobile portrait UX and responsive layout.

Guiding Philosophy & Standards
Enforce .NET 9.0 only. Builds must fail if a different SDK major is used.
Use imperative, minimal rules: Required rules take precedence. All other rules are Preferred or Informational to be decided later.
Enforce SOLID and appropriate GoF patterns; prefer simple, small, well-factored code.
Automate operations using CLI commands only; one-line commands at a time for human execution.
Do not create extra markdown or PowerShell files during conversations. Azure deployment files are the only exception.

Architecture & Maintenance
Use Vertical Slice Architecture with Clean Architecture boundaries where complexity requires separation.
Limit files to ≤500 lines. Enforce via linters or pre-commit checks.
Repository layout at root:
/src/Po.AppName.Api
/src/Po.AppName.Client (Blazor Wasm)
/src/Po.AppName.Shared
/tests/Po.AppName.UnitTests
/tests/Po.AppName.IntegrationTests
/docs containing PRD.MD, STEPS.MD, README.MD only
/scripts for CLI helpers only
Naming: project and table names must follow Po.AppName pattern exactly.
Provide small canonical examples and one-line anti-patterns inline in code comments where rules are non-obvious.

API Observability Error Handling
Expose Swagger/OpenAPI from project start and document endpoints used for manual testing.
Expose mandatory /api/health with readiness and liveness semantics.
Implement global exception handling middleware that transforms all errors into RFC 7807 Problem Details responses. Never return raw exception messages or stack traces.
Use Serilog for structured logging and configure sensible local sinks; follow .NET best practices for telemetry integration.
Add automated checks in CI for presence of Swagger, /api/health, and Problem Details middleware.



Data Persistence Frontend
Default persistence: Azure Table Storage. Use Azurite for local development. Alternative stores require explicit specification and approval.
Table naming pattern: PoAppName[TableName].
Blazor Client: start with built-in components; adopt Radzen.Blazor only for advanced scenarios justified by UX need.
Ensure responsive design that prioritizes mobile portrait experience: fluid grid, touch-friendly controls, readable typography, appropriate breakpoints.
Test main flows on desktop and narrow-screen mobile emulation to validate layout and interactions.

Testing Workflow
Follow TDD: write a failing xUnit test first then implement code. Maintain unit and integration tests separately.
xUnit for unit and integration tests. Integration tests must include setup and teardown to leave no lingering data.
E2E: Playwright MCP with TypeScript; E2E tests are executed manually by you and are not included in CI.
Create a minimal, easy-to-invoke set of API methods surfaced in Swagger for manual verification during development and QA.
Ensure database-using tests run isolated instances against Azurite or disposable test tables and clean up after themselves.

Tooling Checks and Enforcement
Enforce formatting with dotnet format as a pre-commit or CI gate; fail builds on format errors.
Add CI checks to validate: .NET SDK major version is 9, required ports are configured, project prefix conforms to Po.AppName, /api/health exists, and Problem Details middleware is present.
Provide one-line CLI commands in comments for required tasks only; avoid multi-step scripts in conversation.
Keep rules concise and machine-checkable; tag rules as Required, Preferred, or Informational where helpful for future edits.

Implementation Notes
All actionable examples must be one-line CLI commands unless an Azure deployment file is required.
Do not create additional documentation files beyond PRD.MD, STEPS.MD, and README.MD.
I will accept edits to enforcement levels and preferred tools later; current document encodes your responses and strict constraints provided.


