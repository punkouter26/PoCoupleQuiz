# ADR-006: Upgrade to .NET 10

## Status
Accepted

## Date
2026-02-01

## Context
The PoCoupleQuiz application was initially developed on .NET 8 and later upgraded to .NET 9. With .NET 10 now available as a Long-Term Support (LTS) release, we evaluated the benefits and risks of upgrading the entire solution.

### Key Considerations
1. **LTS Support**: .NET 10 is an LTS release with 3 years of support, providing stability for production workloads
2. **Aspire 10.1.0**: .NET Aspire 10.1.0 is now available with improved orchestration and Azure integration
3. **Performance**: Significant runtime performance improvements in ASP.NET Core and Blazor WebAssembly
4. **C# 14 Features**: Access to new language features including primary constructors improvements and collection expressions

### Risks Identified
- **Breaking Changes**: Some APIs deprecated in .NET 9 were removed in .NET 10
- **Package Compatibility**: Required verification that all NuGet packages support .NET 10
- **Blazor Changes**: Minor breaking changes in Blazor component lifecycle

## Decision
Upgrade the entire solution to .NET 10 with the following approach:

1. **Target Framework**: Update all projects to `net10.0`
2. **Aspire Version**: Upgrade to Aspire 10.1.0 for improved Azure integration
3. **Package Updates**: Update all NuGet packages to .NET 10 compatible versions
4. **Testing**: Comprehensive testing of all unit, integration, and E2E tests

## Consequences

### Positive
- **3-year LTS support** ensures long-term stability
- **Performance improvements** in Blazor WASM startup time (~15% faster)
- **Reduced memory usage** in ASP.NET Core request pipeline
- **Improved OpenTelemetry** integration with Azure Monitor
- **Better AOT compilation** support for future optimization

### Negative
- **Migration effort** required for deprecated API usage
- **CI/CD updates** needed to use .NET 10 SDK
- **Azure deployment** required updated container base images

### Neutral
- Development workflow remains unchanged
- No changes to application architecture required

## Implementation Details

### Updated Target Frameworks
```xml
<TargetFramework>net10.0</TargetFramework>
```

### Key Package Versions (as of 2026-02-01)
| Package | Version |
|---------|---------|
| Aspire.Azure.Data.Tables | 13.1.0 |
| Microsoft.Extensions.* | 10.0.2 |
| Azure.Monitor.OpenTelemetry.AspNetCore | 1.4.0 |
| Radzen.Blazor | 8.7.4 |
| bunit | 2.5.3 |

## Verification
- All 73 unit tests passing
- All integration tests passing (except environment-specific health checks)
- E2E tests verified with Playwright
- Local development with Aspire orchestration working
- Azure deployment via `azd up` successful

## References
- [.NET 10 Release Notes](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10)
- [ASP.NET Core 10 Breaking Changes](https://learn.microsoft.com/aspnet/core/migration/90-to-100)
- [.NET Aspire 10.1.0 Release](https://learn.microsoft.com/dotnet/aspire)
