# Developer Walkthrough

Welcome to PoCoupleQuiz! This guide will help you understand the codebase and start contributing quickly.

## Table of Contents

1. [Quick Start](#quick-start)
2. [Project Overview](#project-overview)
3. [Understanding the Game Flow](#understanding-the-game-flow)
4. [Key Files to Know](#key-files-to-know)
5. [Making Your First Change](#making-your-first-change)
6. [Testing Your Changes](#testing-your-changes)
7. [Common Tasks](#common-tasks)

---

## Quick Start

```powershell
# 1. Clone and navigate
cd PoCoupleQuiz

# 2. Start everything with Aspire (includes Azurite)
dotnet run --project PoCoupleQuiz.AppHost

# 3. Open the Aspire Dashboard (URL shown in console)
# Usually: https://localhost:17011

# 4. Click on the "server" resource to open the app
```

**That's it!** The app runs with mock question generation locally (no Azure OpenAI needed).

---

## Project Overview

### Solution Structure

```
PoCoupleQuiz/
├── PoCoupleQuiz.AppHost/      # 🚀 Start here - Aspire orchestration
├── PoCoupleQuiz.Client/       # 🎨 Blazor WASM frontend
│   ├── Pages/                 # Route-able pages
│   ├── Shared/                # Reusable components
│   └── Services/              # HTTP client wrappers
├── PoCoupleQuiz.Server/       # 🌐 ASP.NET Core API
│   ├── Controllers/           # API endpoints
│   └── Middleware/            # Request processing
├── PoCoupleQuiz.Core/         # 💼 Business logic (no HTTP/UI)
│   ├── Models/                # Domain entities
│   ├── Services/              # Business services
│   └── Validators/            # Input validation
├── PoCoupleQuiz.ServiceDefaults/ # ⚙️ Shared Aspire config
├── PoCoupleQuiz.Tests/        # ✅ Test suite
└── e2e-tests/                 # 🎭 Playwright E2E tests
```

### Key Principle: Vertical Slice Architecture

Instead of organizing by technical layer (Controllers, Services, Repositories), we organize by **feature**. Each feature contains everything it needs.

---

## Understanding the Game Flow

### 1. Game Setup (Home Page)

```
User enters team name → Adds player names → Selects difficulty → Clicks Start
```

**Files involved:**
- [Index.razor](../../PoCoupleQuiz.Client/Pages/Index.razor) - UI and form handling
- [Game.cs](../../PoCoupleQuiz.Core/Models/Game.cs) - Game model creation

### 2. King Player Answers

```
First player becomes "King" → Sees the question → Types their answer
```

**Files involved:**
- [Game.razor](../../PoCoupleQuiz.Client/Pages/Game.razor) - Game page orchestration
- [QuestionDisplay.razor](../../PoCoupleQuiz.Client/Shared/QuestionDisplay.razor) - Question UI

### 3. Guessing Players Answer

```
Other players see same question → Try to guess King's answer → Submit
```

**Files involved:**
- [GameStateService.cs](../../PoCoupleQuiz.Core/Services/GameStateService.cs) - Tracks who answered

### 4. Answers Compared

```
AI compares answers semantically → Matches determined → Points awarded
```

**Files involved:**
- [QuestionsController.cs](../../PoCoupleQuiz.Server/Controllers/QuestionsController.cs) - API endpoint
- [AzureOpenAIQuestionService.cs](../../PoCoupleQuiz.Core/Services/AzureOpenAIQuestionService.cs) - AI integration

### 5. Results & Next Round

```
Results shown → Scoreboard updates → Next player becomes King → Repeat
```

**Files involved:**
- [RoundResults.razor](../../PoCoupleQuiz.Client/Shared/RoundResults.razor) - Results display
- [ScoreboardDisplay.razor](../../PoCoupleQuiz.Client/Shared/ScoreboardDisplay.razor) - Live scores

---

## Key Files to Know

| When you want to... | Look at... |
|---------------------|------------|
| Understand the game model | [Core/Models/Game.cs](../../PoCoupleQuiz.Core/Models/Game.cs) |
| Modify the main gameplay | [Client/Pages/Game.razor](../../PoCoupleQuiz.Client/Pages/Game.razor) |
| Change question generation | [Core/Services/AzureOpenAIQuestionService.cs](../../PoCoupleQuiz.Core/Services/AzureOpenAIQuestionService.cs) |
| Add a new API endpoint | [Server/Controllers/](../../PoCoupleQuiz.Server/Controllers/) |
| Register a new service | [Core/Extensions/ServiceCollectionExtensions.cs](../../PoCoupleQuiz.Core/Extensions/ServiceCollectionExtensions.cs) |
| Add shared CSS | [Client/wwwroot/css/app.css](../../PoCoupleQuiz.Client/wwwroot/css/app.css) |
| Configure Aspire resources | [AppHost/AppHost.cs](../../PoCoupleQuiz.AppHost/AppHost.cs) |

---

## Making Your First Change

### Example: Add a new question category

1. **Find the prompt builder** in `AzureOpenAIQuestionService.cs`:

```csharp
private static string BuildPrompt(string? difficulty) =>
    $"""
    Generate a fun relationship quiz question...
    Categories: relationships, hobbies, childhood, future, preferences, values
    """;
```

2. **Add your category** (e.g., "travel"):

```csharp
Categories: relationships, hobbies, childhood, future, preferences, values, travel
```

3. **Test it**:

```powershell
dotnet test PoCoupleQuiz.Tests --filter "QuestionService"
```

4. **Run the app** and generate questions to see your category appear!

---

## Testing Your Changes

### Unit Tests

```powershell
# Run all unit tests
dotnet test PoCoupleQuiz.Tests

# Run specific test class
dotnet test PoCoupleQuiz.Tests --filter "GameStateServiceTests"

# Run with coverage
dotnet test PoCoupleQuiz.Tests --collect:"XPlat Code Coverage"
```

### E2E Tests (Playwright)

```powershell
# First, start the app
dotnet run --project PoCoupleQuiz.AppHost

# In another terminal
cd e2e-tests
npm install
npx playwright test
```

### Manual Testing

1. Start with Aspire: `dotnet run --project PoCoupleQuiz.AppHost`
2. Open Aspire Dashboard
3. Use the app through different scenarios
4. Check logs in the Aspire traces view

---

## Common Tasks

### Add a New Page

1. Create `Client/Pages/MyPage.razor`:
```razor
@page "/mypage"
<h3>My New Page</h3>
```

2. Add to navigation in `MainLayout.razor` (optional)

3. Restart and navigate to `/mypage`

### Add a New API Endpoint

1. Create controller in `Server/Controllers/`:
```csharp
[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get() => Ok("Hello!");
}
```

2. Test with the REST Client: Add request to `docs/api/api-endpoints.http`

### Add a New Service

1. Create interface and implementation in `Core/Services/`:
```csharp
public interface IMyService { /* ... */ }
public class MyService : IMyService { /* ... */ }
```

2. Register in `Core/Extensions/ServiceCollectionExtensions.cs`:
```csharp
services.AddScoped<IMyService, MyService>();
```

### Add a New Package

1. Add to `Directory.Packages.props` (central package management):
```xml
<PackageVersion Include="MyPackage" Version="1.0.0" />
```

2. Reference in project `.csproj`:
```xml
<PackageReference Include="MyPackage" />
```

---

## Getting Help

- **Architecture Decisions**: See [docs/adr/](../adr/)
- **KQL Queries**: See [docs/kql/](../kql/)
- **API Documentation**: See [docs/api/](../api/)
- **Coding Standards**: See [agents.md](../../agents.md)

Happy coding! 🎉
