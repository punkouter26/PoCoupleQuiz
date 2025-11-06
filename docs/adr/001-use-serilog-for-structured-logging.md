# ADR 001: Use Serilog for Structured Logging

## Status
Accepted

## Context
The application requires a robust logging solution that supports:
- Structured logging with rich context
- Multiple output targets (Console, File, Application Insights)
- Configuration via appsettings.json
- Seamless integration with ASP.NET Core
- Strong community support and active maintenance

We evaluated several logging frameworks:
- **Built-in ILogger**: Limited structured logging capabilities, fewer sinks
- **NLog**: Good community support, but less intuitive configuration
- **Serilog**: Excellent structured logging, wide sink ecosystem, fluent API

## Decision
We will use **Serilog** as our primary logging framework for the PoCoupleQuiz application.

## Rationale
1. **Structured Logging**: Serilog's first-class support for structured logs enables rich querying in Application Insights
2. **Sink Ecosystem**: Extensive collection of sinks (Console, File, Application Insights, etc.)
3. **Configuration**: Can be fully configured via appsettings.json without code changes
4. **Performance**: Excellent performance with minimal overhead
5. **ASP.NET Core Integration**: Native integration via Serilog.AspNetCore package
6. **CorrelationId Enrichment**: Easy to enrich logs with Activity.Current.Id for distributed tracing

## Consequences

### Positive
- Rich, queryable logs in Application Insights
- Consistent logging format across all environments
- Easy to add new sinks without code changes
- Better debugging experience with structured properties
- Correlation IDs automatically tracked across requests

### Negative
- Additional NuGet dependencies (Serilog.AspNetCore, sink packages)
- Team must learn Serilog-specific patterns and best practices
- Slight learning curve for structured logging syntax

## Implementation Notes
- Configure Serilog early in Program.cs before the host is built
- Use enrichers for CorrelationId via Activity.Current.Id
- Configure separate sinks for Development (Console, File) and Production (Application Insights)
- Use Serilog's request logging middleware to log HTTP requests with timing

## References
- [Serilog Documentation](https://serilog.net/)
- [Serilog.AspNetCore GitHub](https://github.com/serilog/serilog-aspnetcore)
- [Structured Logging Best Practices](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)
