# Code Quality Metrics Reports

This directory contains timestamped code quality reports for trend tracking and AI-driven analysis.

## Report Format

Each report is a JSON file named `YYYY-MM-DD_metrics.json` containing:

| Section | Description |
|---------|-------------|
| `summary` | High-level metrics (files, lines, averages) |
| `thresholds` | Quality gates (MI ≥ 60, CC ≤ 15) |
| `highComplexityFiles` | Files exceeding CC threshold |
| `lowMaintainabilityFiles` | Files below MI threshold |
| `packageAudit` | Dependency health and upgrades |
| `testCoverage` | Test execution summary |
| `architectureMetrics` | Inheritance depth, coupling |
| `delta` | Changes compared to previous report |
| `aiReadiness` | AI/ML integration status |

## Quality Thresholds

| Metric | Threshold | Action |
|--------|-----------|--------|
| Maintainability Index | < 60 | Refactor required |
| Cyclomatic Complexity | > 15 | Split method/component |
| Depth of Inheritance | > 4 | Use composition |
| Class Coupling | > 30 | Extract interfaces |

## Generating Reports

Reports are generated during the PoTuneBestPractice workflow:

```powershell
# Manual generation (future automation)
dotnet run --project Tools/MetricsGenerator
```

## Trend Analysis

The `delta` section tracks changes between reports:
- `maintainabilityChange`: +/- points since last report
- `complexityChange`: +/- average CC since last report
- `newIssues`: New files exceeding thresholds
- `resolvedIssues`: Files now meeting thresholds

## AI Integration

Reports are structured for consumption by AI monitoring systems:
- JSON format with consistent schema
- ISO 8601 timestamps
- Semantic naming conventions
- Health endpoint correlation via `aiReadiness`
