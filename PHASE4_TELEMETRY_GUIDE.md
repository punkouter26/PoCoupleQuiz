# Phase 4: Debugging & Telemetry - Implementation Guide

## Overview

Phase 4 implements comprehensive structured logging and Application Insights telemetry for PoCoupleQuiz, providing powerful observability for debugging, performance monitoring, and usage analytics.

## ✅ Completed Implementations

### 1. Structured Logging with Serilog

**Configured Sinks:**
- ✅ **Console Sink** - CompactJSON format for dev-friendly logs
- ✅ **File Sink** - Rolling daily logs in `DEBUG/log-YYYYMMDD.txt` (dev-only, 7-day retention)
- ✅ **Application Insights Sink** - Real-time telemetry to Azure

**Configuration Files:**
- `appsettings.json` - Production logging configuration with AI sink
- `appsettings.Development.json` - Debug-level logging for local development

**Key Features:**
- Structured properties for property-based querying in Application Insights
- Automatic log enrichment with machine name, thread ID, and request context
- Shared file logging for parallel test execution compatibility
- Rolling daily logs with size limits (10MB per file)

### 2. Application Insights Integration

**NuGet Packages Added:**
- `Microsoft.ApplicationInsights.AspNetCore` v2.23.0
- `Serilog.Sinks.ApplicationInsights` v4.0.0

**Services Configured:**
- Application Insights telemetry collection
- Custom telemetry middleware for performance tracking
- Request logging with enriched diagnostic context

**Connection String Configuration:**
```json
{
  "ApplicationInsights": {
    "ConnectionString": ""  // Set in Azure Portal or from bicep deployment
  }
}
```

### 3. Client-Side Logging Endpoint

**API Endpoint:** `POST /api/log/client`

**Controller:** `LogController.cs`
- Receives client-side logs from Blazor WebAssembly
- Validates log levels (Trace, Debug, Information, Warning, Error, Critical)
- Enriches logs with client context (URL, user agent, timestamp)
- Supports structured properties for detailed telemetry

**Request Model:**
```json
{
  "level": "Warning",
  "message": "User action failed",
  "url": "https://example.com/game",
  "timestamp": "2025-10-24T10:00:00Z",
  "properties": {
    "userId": "user123",
    "action": "submit_answer"
  }
}
```

**Client-Side Service:** `ServerLoggerService.cs`
- ILogger implementation that sends logs to server
- Only sends Warning and above to reduce noise
- Fire-and-forget pattern to avoid blocking UI
- Silent fallback on logging failures

### 4. Custom Telemetry & Performance Tracking

**Telemetry Middleware:** `TelemetryMiddleware.cs`
- Tracks request duration, response size, status codes
- Logs slow requests (> 1000ms) automatically
- Captures error responses with detailed context
- Enriches logs with structured properties

**Enhanced Controllers:**
- `TeamsController` - Logs team retrieval count
- `GameHistoryController` - Logs game sessions with scores, modes, teams
- All controllers enriched with structured logging scopes

**Telemetry Properties Captured:**
```csharp
- RequestPath, RequestMethod, StatusCode
- DurationMs, ResponseSizeBytes
- UserAgent, RemoteIP
- Team1Name, Team2Name, GameMode
- Team1Score, Team2Score, TotalQuestions
```

### 5. Essential KQL Queries

**File:** `KQL_QUERIES.kql`

**Query 1: User Activity (Last 7 Days)**
- Measures daily active users and sessions
- Calculates average session duration
- Filters out health check noise
- Provides daily breakdown for trend analysis

**Query 2: Top 10 Slowest Requests**
- Identifies performance bottlenecks
- Shows P95 and P99 percentiles
- Highlights requests > 100ms
- Performance rating: Good / Moderate / Slow / Critical

**Query 3: Error Rate (Last 24 Hours)**
- Monitors application health
- Calculates success vs. failure percentage
- Health status: Healthy (< 1%) / Warning (< 5%) / Critical (>= 5%)
- Includes hourly error rate trend analysis

**Bonus Queries:**
- API endpoint usage statistics
- Game activity by mode and scores
- Client-side browser errors
- Performance by geography
- External dependency health

## 📊 Data Captured

### Server-Side Telemetry
| Event | Properties | Purpose |
|-------|-----------|---------|
| HTTP Request | Path, Method, StatusCode, Duration, Size | Performance monitoring |
| Game Saved | Team names, scores, mode, questions | Usage analytics |
| Team Retrieved | Team count | Data access patterns |
| Slow Request | Path, Duration > 1000ms | Performance alerts |
| Error Response | Path, StatusCode 4xx/5xx | Error tracking |

### Client-Side Telemetry
| Event | Properties | Purpose |
|-------|-----------|---------|
| Client Warning | Message, URL, User Agent | Browser issues |
| Client Error | Exception, Stack Trace, Timestamp | Client crashes |
| Client Critical | Message, Properties | Fatal errors |

## 🔧 Configuration Steps

### Local Development Setup

1. **Application Insights (Optional for Local)**
   ```json
   // appsettings.Development.json
   {
     "ApplicationInsights": {
       "ConnectionString": ""  // Leave empty for local dev
     }
   }
   ```

2. **File Logging (Automatic)**
   - Logs written to `DEBUG/log-YYYYMMDD.txt`
   - Rolling daily, 7-day retention
   - Shared mode enabled for test compatibility

3. **Console Logging (Always On)**
   - CompactJSON format for structured data
   - Color-coded by log level (in VS Code terminal)

### Azure Deployment Setup

1. **Get Application Insights Connection String**
   ```bash
   # From Azure Portal
   Azure Portal > Application Insights > Your Resource > Overview > Connection String
   
   # From Azure CLI
   az monitor app-insights component show \
     --resource-group rg-pocouplequiz-dev \
     --app pocouplequiz-dev-appinsights \
     --query connectionString -o tsv
   ```

2. **Update Configuration**
   ```json
   // appsettings.json or Azure App Service Configuration
   {
     "ApplicationInsights": {
       "ConnectionString": "InstrumentationKey=...;IngestionEndpoint=..."
     },
     "Serilog": {
       "WriteTo": [
         {
           "Name": "ApplicationInsights",
           "Args": {
             "connectionString": "InstrumentationKey=...;IngestionEndpoint=..."
           }
         }
       ]
     }
   }
   ```

3. **Verify Telemetry Flow**
   - Azure Portal > Application Insights > Live Metrics
   - Should see requests, dependencies, and custom events in real-time

## 📈 Using KQL Queries

### Running Queries in Azure Portal

1. Navigate to **Azure Portal > Application Insights > Logs**
2. Copy query from `KQL_QUERIES.kql`
3. Paste into query editor
4. Click **Run**
5. View results in table or chart format

### Saving Queries

1. After running a query, click **Save**
2. Name: "Daily Active Users" (for Query 1)
3. Category: "Usage Analytics"
4. Pin to dashboard for quick access

### Creating Alerts

Based on KQL query results:

1. Azure Portal > Application Insights > Alerts > New Alert Rule
2. Select metric: "Custom log search"
3. Paste KQL query
4. Set threshold (e.g., ErrorRate > 5%)
5. Configure action group (email/SMS)
6. Save alert rule

## 🎯 Recommended Alerts

### 1. High Error Rate
```
Condition: Error rate > 5% for 5 minutes
Severity: High
Action: Email on-call team
```

### 2. Slow Performance
```
Condition: P95 response time > 2000ms for 10 minutes
Severity: Medium
Action: Email dev team
```

### 3. Availability
```
Condition: No requests for 5 minutes (business hours)
Severity: Critical
Action: Page on-call engineer
```

### 4. Dependency Failures
```
Condition: Azure Storage/OpenAI failure rate > 10%
Severity: High
Action: Email ops team
```

## 🧪 Testing Telemetry Locally

### 1. Verify File Logging
```powershell
# Start application
dotnet run --project PoCoupleQuiz.Server

# Check log file created
ls DEBUG/

# Tail logs in real-time (PowerShell)
Get-Content DEBUG/log-20251024.txt -Wait -Tail 20
```

### 2. Verify Console Logging
```powershell
# Run application and watch console for JSON logs
# Look for structured properties in output
```

### 3. Test Client Logging Endpoint
```powershell
# Using curl or PowerShell
$body = @{
    level = "Warning"
    message = "Test client log"
    url = "https://localhost:5001/test"
    timestamp = (Get-Date).ToUniversalTime().ToString("o")
    properties = @{
        testKey = "testValue"
    }
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/log/client" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

### 4. Verify Structured Properties
```powershell
# Check DEBUG log file for structured properties
Get-Content DEBUG/log-20251024.txt | Select-String "Team1Name"
```

## 📋 Telemetry Best Practices

### DO ✅
- Use structured logging with properties, not string interpolation
- Log at appropriate levels (Debug for dev, Information for events, Warning for issues)
- Include correlation IDs for distributed tracing
- Filter out noisy endpoints (health checks) from metrics
- Use scopes for grouping related log statements
- Redact sensitive data (passwords, tokens) from logs

### DON'T ❌
- Don't log every single request - use sampling for high-volume endpoints
- Don't include PII (personal identifiable information) in logs
- Don't log full request/response bodies - use summaries
- Don't block application flow on logging failures
- Don't log the same information multiple times

### Example: Good vs. Bad Logging

**❌ Bad:**
```csharp
_logger.LogInformation("User logged in: " + userName + " at " + DateTime.Now);
```

**✅ Good:**
```csharp
using (_logger.BeginScope(new Dictionary<string, object?> 
{
    ["UserId"] = userId,
    ["LoginMethod"] = "OAuth"
}))
{
    _logger.LogInformation("User logged in successfully");
}
```

## 🔍 Troubleshooting

### Logs Not Appearing in Application Insights

1. **Check Connection String**
   - Verify `ApplicationInsights:ConnectionString` is set correctly
   - Test with Live Metrics in Azure Portal

2. **Check Serilog Configuration**
   - Ensure ApplicationInsights sink is configured in appsettings.json
   - Verify minimum log level allows your events through

3. **Check Network**
   - Application Insights requires outbound HTTPS (443) access
   - Check firewall rules if running behind corporate proxy

### Client Logs Not Reaching Server

1. **Check CORS Configuration**
   - Blazor WASM must be able to call `/api/log/client`
   - Verify controller route is correct

2. **Check Client Logger Registration**
   - Ensure `ServerLoggerProvider` is registered in client `Program.cs`
   - Verify HttpClient is available in DI container

3. **Check Browser Console**
   - Open DevTools > Network tab
   - Look for POST requests to `/api/log/client`
   - Check for 4xx/5xx errors

### Performance Impact

**Logging Overhead:**
- File logging: ~1-5ms per request
- Application Insights: ~10-20ms per request (async)
- Console logging: ~0.5-2ms per request

**Mitigation:**
- Use sampling for high-volume endpoints (> 1000 RPS)
- Configure minimum log levels appropriately
- Use async logging (enabled by default)
- Implement log filtering for noisy endpoints

## 📚 Additional Resources

- [Application Insights Overview](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [Serilog Best Practices](https://github.com/serilog/serilog/wiki/Best-Practices)
- [KQL Quick Reference](https://learn.microsoft.com/en-us/azure/data-explorer/kql-quick-reference)
- [Structured Logging in .NET](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/)

## ✅ Phase 4 Complete

All telemetry and debugging infrastructure is now in place:
- ✅ Structured logging with Serilog (Console, File, Application Insights sinks)
- ✅ Application Insights integration configured
- ✅ Client-side logging endpoint (`POST /api/log/client`)
- ✅ Custom telemetry middleware for performance tracking
- ✅ Enhanced controllers with structured logging
- ✅ Three essential KQL queries for usage, performance, and health
- ✅ Comprehensive documentation and testing guide

**Next Steps:**
1. Deploy to Azure and configure Application Insights connection string
2. Set up recommended alerts in Azure Portal
3. Pin KQL queries to dashboard
4. Monitor Live Metrics during initial deployment
5. Review logs regularly and refine based on actual usage patterns
