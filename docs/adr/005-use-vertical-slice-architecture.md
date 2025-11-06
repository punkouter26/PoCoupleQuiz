# ADR 005: Use Vertical Slice Architecture

## Status
Accepted

## Context
The application architecture must balance:
- **Maintainability**: Easy to understand and modify
- **Scalability**: Support growing feature sets
- **Testability**: Enable comprehensive automated testing
- **Clarity**: Clear code organization for onboarding developers

Traditional layered architecture (Controllers → Services → Repositories) has limitations:
- Features scattered across multiple layers
- Difficult to understand feature scope
- High coupling between layers
- Changes often require modifying multiple layers

We evaluated several architectural approaches:
- **Layered Architecture**: Traditional N-tier (Presentation → Business → Data)
- **Clean Architecture**: Onion architecture with dependency inversion
- **Vertical Slice Architecture**: Feature-centric organization
- **Modular Monolith**: Domain-driven design with bounded contexts

## Decision
We will use **Vertical Slice Architecture** for organizing backend features in the `PoCoupleQuiz.Server` project.

## Rationale
1. **Feature Cohesion**: All code for a feature lives in one folder
2. **Minimal Coupling**: Slices are independent, reducing ripple effects
3. **Easier Onboarding**: New developers can understand a feature in isolation
4. **Testability**: Each slice can be tested independently
5. **Alignment with CQRS**: Natural fit for command/query separation
6. **Reduced Abstractions**: No need for generic repositories or services
7. **Pragmatic**: Simpler than Clean Architecture for small-to-medium applications

## Consequences

### Positive
- **Discoverability**: All feature code in `/Features/{FeatureName}/`
- **Independence**: Modifying one slice rarely affects others
- **Clear Boundaries**: Each slice has its own models, validators, handlers
- **Faster Development**: Less ceremony, fewer abstractions
- **Better Testing**: Integration tests can target specific slices
- **SOLID Compliance**: Single Responsibility per slice

### Negative
- **Potential Duplication**: Shared logic may be duplicated across slices
- **Learning Curve**: Developers familiar with layered architecture need to adjust
- **File Organization**: More files/folders (one folder per feature)
- **Tooling**: Some IDE features work better with traditional layering

## Structure Example

```
PoCoupleQuiz.Server/
  Features/
    GetGame/
      Endpoint.cs          // Minimal API endpoint
      Query.cs             // Request DTO
      Handler.cs           // Business logic (future: MediatR handler)
      Validator.cs         // Input validation
      GameDto.cs           // Response DTO
    StartGame/
      Endpoint.cs
      Command.cs
      Handler.cs
      Validator.cs
    SubmitAnswer/
      Endpoint.cs
      Command.cs
      Handler.cs
      Validator.cs
  Shared/
    Models/                // Shared domain models
    Services/              // Cross-cutting services (logging, storage)
    Middleware/            // Shared middleware
```

## Implementation Guidelines

### 1. Feature Organization
- Each feature gets its own folder under `/Features/`
- Folder name = feature name (e.g., `GetLeaderboard`, `CreatePlayer`)
- Use PascalCase for folder names

### 2. File Naming
- `Endpoint.cs`: Minimal API registration
- `Query.cs` or `Command.cs`: Request DTO (CQRS pattern)
- `Handler.cs`: Business logic (future: `IRequestHandler<TRequest, TResponse>`)
- `Validator.cs`: FluentValidation validator (future enhancement)
- `{Feature}Dto.cs`: Response DTOs specific to this feature

### 3. Shared Code
- Put truly shared code in `/Shared/`
- Examples: Domain models, base classes, utilities
- Avoid premature abstraction—let shared code emerge naturally

### 4. Dependencies
- Slices can depend on `/Shared/` but not on other slices
- Use dependency injection for cross-cutting concerns (logging, storage)
- Keep external dependencies minimal

### 5. Testing
- Unit tests for Handlers (mock dependencies)
- Integration tests for Endpoints (using WebApplicationFactory)
- Place tests in `PoCoupleQuiz.Tests/Features/{FeatureName}/`

## CQRS Integration (Future)

We plan to integrate **MediatR** for cleaner CQRS:
```csharp
// Query
public record GetGameQuery(string GameId) : IRequest<GameDto>;

// Handler
public class GetGameHandler : IRequestHandler<GetGameQuery, GameDto>
{
    public async Task<GameDto> Handle(GetGameQuery request, CancellationToken ct) { ... }
}

// Endpoint
app.MapGet("/api/games/{gameId}", async (string gameId, IMediator mediator) => 
{
    return await mediator.Send(new GetGameQuery(gameId));
});
```

## When to Use Shared Code

| Use Case | Location | Reason |
|----------|----------|--------|
| Domain models (Player, Game) | `/Shared/Models/` | Used across multiple features |
| Table Storage service | `/Shared/Services/` | Cross-cutting infrastructure |
| Custom middleware | `/Shared/Middleware/` | Request pipeline concerns |
| Extension methods | `/Shared/Extensions/` | Utility functions |
| Feature-specific DTOs | `/Features/{Feature}/` | Only used by one feature |

## Alternatives Considered

### Clean Architecture (Onion)
- **Pros**: Strong separation of concerns, testable, DDD-friendly
- **Cons**: High ceremony, many abstractions, steep learning curve
- **Why not chosen**: Too complex for a small-to-medium application

### Layered Architecture
- **Pros**: Familiar, well-understood, good IDE tooling
- **Cons**: Features scattered across layers, high coupling, hard to change
- **Why not chosen**: Leads to rigid, hard-to-maintain code

### Modular Monolith
- **Pros**: Strong boundaries, great for large systems
- **Cons**: Overkill for single-team projects, requires strict discipline
- **Why not chosen**: Too heavy for our current scale

## References
- [Vertical Slice Architecture by Jimmy Bogard](https://jimmybogard.com/vertical-slice-architecture/)
- [REPR Pattern (Request-Endpoint-Response)](https://deviq.com/design-patterns/repr-design-pattern)
- [MediatR for CQRS](https://github.com/jbogard/MediatR)
- [Feature Folders in ASP.NET Core](https://scottsauber.com/2016/04/25/feature-folder-structure-in-asp-net-core/)
