AI Assistant Development Rules
Section 1: General Principles & Core Workflow
Step-Driven Process: Strictly adhere to the ~10 high-level steps defined in steps.md if it exists (if it does not exist then follow only user prompts)
Progress Tracking: Mark completed steps in steps.md using the format: - [x] Step X: Description.
Product Requirements: Reference prd.md in the root directory for all product requirements. Never modify prd.md. (if prd.md does not exist then just do the work the user prompts)
Design Philosophy: Prioritize simplicity, functional correctness, and future expandability in all design decisions. Avoid premature optimization.
Proactive Suggestions: After completing the assigned task, offer 5 relevant and helpful subsequent tasks that could be performed.
File Cleanup: When encountering unused files or code, list all potentially removable items at once and ask for user confirmation before deleting them.
Logging & Diagnostics Workflow
Comprehensive Logging: Implement a robust logging strategy that outputs to the Console, Serilog, and Application Insights.
 File:
A single log.txt file must be created or overwritten in the root project directory on each application run.
It must contain the most recent, detailed information from both the server and client (if feasible) to aid in debugging.
All log entries must include timestamps, component/class names, and operational context. Log key decisions, state changes, and detailed information for critical events like database calls and API requests.
User Confirmation: Request user confirmation before proceeding to the next step with a clear summary:
I've completed Step X: [Step Description].
The code compiles, all relevant tests pass, and the log file shows no errors.
Would you like me to:
Make adjustments to the current step
Proceed to Step Y: [Next Step Description]
Section 2: Project & Solution Structure
Solution Naming: The solution name will be derived from prd.md Title and must start with Po (e.g., PoYourSolutionName).
Root Directory: All project files and folders will be contained within a root directory named after the solution (e.g., PoYourSolutionName/).
Solution File: The solution file (PoYourSolutionName.sln) will be located in the root directory.
Core Files: The following files might exist in the root directory:
steps.md: For tracking high-level development progress 
prd.md: Contains the product requirements and solution name.
log.txt: A single debug log file, overwritten on each run.
Project Organization: Each project must reside in its own folder at the root level, named appropriately (e.g., PoYourSolutionName.Api/, PoYourSolutionName.Client/). The corresponding .csproj file will be inside its respective folder.
Standard Folders:
.github/workflows/: Contains all CI/CD GitHub Action workflow files.
.vscode/: Contains launch.json and tasks.json configured for local F5 debugging.
AzuriteData/: For local Azurite table storage data (must be added to .gitignore).
Version Control: Create a standard .gitignore file for .NET projects at the root.




















Section 3: C#/.NET & API Development
General C#/.NET Principles
Framework: Target the .NET 9.x framework (or the latest stable version).
Architecture:
Decide between Vertical Slice Architecture (feature folders/CQRS) or Onion Architecture based on prd.md and complexity.
If the application is simple and these patterns are overkill, implement a simpler, well-organized structure.
Coding Standards:
Use SOLID principles and C# Design Patterns.
Add comments in class files to explain the specific SOLID principle or GoF design pattern being used (e.g., // Using Repository Pattern to abstract data access).
Keep classes under 500 lines where possible.
Dependency Injection (DI): Follow standard DI practices (Transient, Scoped, Singleton) and register all services in Program.cs or a dedicated extension method.
Localization: All UI text, logs, and messages must be in English unless explicitly required by prd.md.
API-Specific Implementation (.NET Core Web API)
Project Setup: Create a .NET Core Web API project using dotnet new webapi.
Error Handling & Reliability:
Implement a global exception handler middleware to catch unhandled exceptions.
Use try/catch blocks at service boundaries (e.g., database calls, external API calls).
Return appropriate and consistent HTTP status codes from API endpoints.
Consider implementing the Circuit Breaker pattern for calls to external services that may be unreliable.
Feature Toggles: Consider using configuration-based feature toggles (appsettings.json or Azure configuration) for enabling/disabling features without redeploying.






Section 4: Testing
Framework: Use XUnit for all unit and integration tests.
Workflow: Create services and their corresponding integration tests first. Verify that all tests pass before beginning UI implementation.
Test Coverage: Write tests for all new business logic and core functionality in the API (controllers, services, data access).
Connection Tests: Create dedicated tests to verify connections to external services (like Azure Table Storage) using test data and credentials.
Debugging: Include descriptive debug statements in test methods to aid in troubleshooting.
































Section B1: Blazor Development (Web UI)
Project Setup: Create a hosted Blazor WebAssembly project, where the client app is hosted by the ASP.NET Core server app.
Page Configuration: Set the application name (from prd.md) as the <title> for all pages, so it appears correctly in browser tabs and bookmarks.
UI/UX:
Implement a responsive design that works on various screen sizes.
The Home.razor component should serve as the primary landing page.
Use the Radzen Blazor UI library if the application's complexity warrants enhanced controls.
Mandatory Diagnostics Page (
Create a Diag.razor page accessible at the /diag route.
The page must communicate with the server project to verify all critical connections.
Display the status of each check in a clear grid format (e.g., Green for OK, Red for Error).
Verify and display the status of:
Data connections (Azure Table Storage / Azurite).
API health checks.
Internet connectivity.
Authentication services (if used).
Any other critical external dependencies.
Log all diagnostic results to all configured logging targets.















Section B2: Azure Deployment & Implementation
Phasing: Develop and verify all functionality locally first. Azure deployment is a final step.
Resource Groups:
Use the existing PoShared resource group for shared Azure resources (App Service Plan, Azure OpenAI, Log Analytics etc).
Use azd up for the initial deployment, which will create a new, application-specific resource group named after the solution.
Deployment Workflow (CI/CD):
Create a YAML file in the root for azd to define the App Service and other app-specific resources (Azure Table Storage)
After the initial azd up, use GitHub Actions for all subsequent CI/CD deployments to the application's resource group.
Management & Best Practices:
Prefer using the Azure CLI (az) and GitHub CLI (gh) for all configuration and information retrieval over the Azure Portal UI manual config
Select the minimum viable Azure resource tier to manage costs.
Tag all created Azure resources for identification and cost tracking. Implement retry policies and use connection pooling for interactions with Azure services.
Data & Configuration
Primary Database: Use Azure Table Storage as the primary database.
Local Development: Use the Azurite emulator for local table storage development.
Configuration & Secrets:
Local: Store development-specific connection strings (e.g., for Azurite, SAS tokens) in appsettings.Development.json.
Azure: Store production secrets and connection strings in Azure App Service Configuration (as environment variables). General settings can remain in appsettings.json.
Monitoring (Application Insights)
Integration: Use the shared Application Insights resource in the PoShared resource group.
Telemetry: Track requests, dependencies, exceptions, and performance metrics.
Analytics: Connect Application Insights to the existing Log Analytics resource in the PoShared resource group for advanced querying.