# KQL Query Library for Po.CoupleQuiz

This folder contains **Kusto Query Language (KQL)** queries for analyzing Application Insights telemetry data in Azure Monitor.

## 📂 Query Organization

All queries are numbered and categorized for easy navigation:

| Query File | Category | Description |
|------------|----------|-------------|
| `01-user-activity.kql` | **User Analytics** | Active users, sessions, and engagement metrics (last 7 days) |
| `02-server-performance.kql` | **Performance** | Top 10 slowest API endpoints with P50/P95/P99 latency |
| `03-server-stability.kql` | **Reliability** | Request error rate percentage by endpoint (last 24 hours) |
| `04-client-errors.kql` | **Debugging** | Top client-side exceptions grouped by browser |
| `05-e2e-trace-funnel.kql` | **Tracing** | End-to-end trace correlation using W3C Trace Context |
| `06-custom-metric-game-duration.kql` | **Custom Metrics** | Game duration by difficulty (timechart visualization) |
| `07-custom-activity-traces.kql` | **Custom Tracing** | Custom ActivitySource spans for business logic |
| `08-custom-metric-question-latency.kql` | **Custom Metrics** | AI question generation latency monitoring |
| `09-custom-metric-active-players.kql` | **Custom Metrics** | Real-time active player count (observable gauge) |
| `10-custom-metric-storage-performance.kql` | **Custom Metrics** | Azure Table Storage operation performance |
| `11-realtime-player-sessions.kql` | **Live Dashboard** | Active game sessions with player engagement (30min window) |
| `12-openai-latency-percentiles.kql` | **AI Performance** | OpenAI P50/P95/P99 latency with SLO breach detection |
| `13-error-correlation.kql` | **Root Cause** | Exception correlation with triggering requests |
| `14-deployment-impact.kql` | **DevOps** | Before/after deployment metric comparison |
| `15-user-journey-funnel.kql` | **Conversion** | Game flow funnel with drop-off analysis |

---

## 🚀 Quick Start

### Prerequisites
1. **Application Insights** resource provisioned in Azure
2. **Telemetry data** flowing from deployed application
3. Access to **Azure Portal** or **Azure Monitor Logs**

### Running Queries

**Option 1: Azure Portal**
1. Navigate to **Application Insights** → **Logs**
2. Copy query from `.kql` file
3. Paste into query editor
4. Click **Run**

**Option 2: VS Code (Azure Monitor Extension)**
1. Install **Azure Monitor** extension
2. Open `.kql` file
3. Right-click → **Run Query**

**Option 3: Azure CLI**
```bash
# Run query from file
az monitor app-insights query \
  --app "Po.CoupleQuiz" \
  --analytics-query "$(cat 01-user-activity.kql)" \
  --offset 7d
```

---

## 📊 Query Categories

### 1. User Analytics
**Goal**: Understand user behavior and engagement

- **01-user-activity.kql**: Daily active users (DAU), sessions, page views per user

**Key Metrics**:
- Unique users (distinct `user_Id`)
- Unique sessions (distinct `session_Id`)
- Page views per user (engagement indicator)

**Use Cases**:
- Track growth over time
- Identify usage patterns
- Validate feature adoption

---

### 2. Performance Monitoring
**Goal**: Identify slow endpoints and optimize response times

- **02-server-performance.kql**: Top 10 slowest endpoints by P95 latency

**Key Metrics**:
- **P50/P95/P99** latency percentiles
- Average duration
- Request count and success rate

**Use Cases**:
- Performance benchmarking
- API optimization priorities
- SLA compliance verification

**Example Output**:
| Endpoint | P95_ms | RequestCount | SuccessRate |
|----------|--------|--------------|-------------|
| `POST /api/questions/generate` | 1234.56 | 5000 | 98.5% |

---

### 3. Reliability & Stability
**Goal**: Monitor application health and error rates

- **03-server-stability.kql**: Hourly error rate by endpoint

**Key Metrics**:
- Total requests vs. failed requests
- Error rate percentage
- 5xx (server errors) vs. 4xx (client errors)

**Use Cases**:
- Incident detection
- Error budgeting (SLO tracking)
- Deployment validation (compare before/after)

---

### 4. Client-Side Debugging
**Goal**: Identify and fix browser-specific issues

- **04-client-errors.kql**: Top exceptions by browser type

**Key Metrics**:
- Exception type and message
- Browser distribution
- Affected user count

**Use Cases**:
- Browser compatibility testing
- JavaScript error prioritization
- User impact assessment

---

### 5. End-to-End Tracing
**Goal**: Debug complete user journeys across client and server

- **05-e2e-trace-funnel.kql**: Trace single request using `operation_Id`

**How W3C Trace Context Works**:
1. Client initiates page view → generates `operation_Id`
2. Client calls API → same `operation_Id` propagated in headers
3. Server processes request → logs with same `operation_Id`
4. Server calls dependencies (DB, APIs) → parent/child relationship

**Usage**:
```kql
let operationId = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";
// Rest of query...
```

**Use Cases**:
- Debug failed requests
- Understand dependency chains
- Measure end-to-end latency

---

### 6. Custom Metrics (Business Telemetry)
**Goal**: Track business-specific KPIs

| Query | Custom Metric | Tags/Dimensions |
|-------|---------------|-----------------|
| `06-custom-metric-game-duration.kql` | `po.couplequiz.game.duration` | `difficulty` |
| `08-custom-metric-question-latency.kql` | `po.couplequiz.questions.generation_latency` | `question_source`, `difficulty` |
| `09-custom-metric-active-players.kql` | `po.couplequiz.players.active` | N/A (gauge) |
| `10-custom-metric-storage-performance.kql` | `po.couplequiz.storage.latency` | `operation`, `table_name` |

**How to Use Custom Metrics**:
1. Metrics are created via `Meter.CreateHistogram()` / `Meter.CreateCounter()`
2. Automatically exported to Application Insights as `customMetrics`
3. Query using `customMetrics` table with `name` filter

**Example Visualization** (timechart):
```kql
customMetrics
| where name == "po.couplequiz.game.duration"
| summarize avg(value) by bin(timestamp, 1h), tostring(customDimensions["difficulty"])
| render timechart
```

---

### 7. Custom Activity Traces (Business Logic)
**Goal**: Track custom operations using ActivitySource

- **07-custom-activity-traces.kql**: Custom spans for business workflows

**How Custom Activities Work**:
1. Create `Activity` using `ActivitySource.StartActivity("OperationName")`
2. Add tags: `activity.SetTag("gameId", "123")`
3. Activity duration and status automatically tracked
4. Exported to Application Insights as `dependencies` (type: `InProc`)

**Use Cases**:
- Measure business operation latency (e.g., "ScoreCalculation")
- Debug multi-step workflows
- Monitor internal service calls

---

## 🛠️ Customization

### Modifying Time Ranges
All queries use relative time ranges (e.g., `ago(7d)`, `ago(24h)`).

**Change to absolute range**:
```kql
| where timestamp between(datetime(2025-11-01) .. datetime(2025-11-07))
```

### Adding Filters
Filter by custom properties:
```kql
| where customDimensions["difficulty"] == "hard"
| where customDimensions["gameId"] == "abc123"
```

### Exporting Results
**To CSV**:
1. Run query in Azure Portal
2. Click **Export** → **Export to CSV**

**To Power BI**:
1. Run query
2. Click **Export** → **Open in Power BI**

**To Azure Data Explorer**:
```bash
# Stream results to Kusto cluster
.set-or-append TableName <| 
YourQuery
```

---

## 📈 Creating Dashboards

### Option 1: Azure Dashboard
1. Run query in Application Insights Logs
2. Click **Pin to dashboard**
3. Select existing or create new dashboard

### Option 2: Azure Workbooks
1. Application Insights → **Workbooks** → **New**
2. Add query blocks from `.kql` files
3. Add visualizations (timechart, barchart, piechart)
4. Save as shared workbook

### Option 3: Grafana
1. Install **Azure Monitor** plugin
2. Add Application Insights datasource
3. Create panel with KQL query
4. Configure refresh interval

---

## 🔔 Setting Up Alerts

### Create Alert from Query

**Example: Alert on High Error Rate**
```kql
requests
| where timestamp > ago(5m)
| summarize ErrorRate = 100.0 * countif(success == false) / count()
| where ErrorRate > 5.0  // Alert if error rate > 5%
```

**Steps**:
1. Run query in Application Insights Logs
2. Click **New alert rule**
3. Set threshold: `ErrorRate > 5.0`
4. Configure action group (email, SMS, webhook)
5. Save rule

**Recommended Alerts**:
- Error rate > 5% (critical)
- P95 latency > 2000ms (warning)
- Active users drop > 50% (critical)
- Client exceptions spike (warning)

---

## 📚 Additional Resources

- [KQL Quick Reference](https://learn.microsoft.com/en-us/azure/data-explorer/kql-quick-reference)
- [Application Insights Schema](https://learn.microsoft.com/en-us/azure/azure-monitor/app/data-model)
- [W3C Trace Context Spec](https://www.w3.org/TR/trace-context/)
- [OpenTelemetry Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/)

---

## 🤝 Contributing

When adding new queries:
1. Use numbered prefix (e.g., `11-new-query.kql`)
2. Include header comment block with description, use case, metrics
3. Add entry to this README
4. Test query with sample data
5. Document required custom instrumentation (if any)

---

**Last Updated**: November 6, 2025  
**Maintained By**: Po.CoupleQuiz Development Team
