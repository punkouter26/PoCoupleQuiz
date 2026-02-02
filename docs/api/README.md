# API Documentation

## Overview

PoCoupleQuiz exposes a RESTful API for game operations. The API is documented using OpenAPI and can be explored interactively.

## Swagger UI (Interactive Explorer)

When running in development mode, an interactive API explorer is available at:

```
https://localhost:7001/swagger
```

**Features:**
- 📋 Browse all endpoints with descriptions
- 🧪 Try out requests directly in the browser
- 📝 View request/response schemas
- 🔍 Search and filter endpoints

## OpenAPI / Swagger

Raw OpenAPI specification is available at:

```
https://localhost:7001/swagger/v1/swagger.json
https://localhost:7001/openapi/v1.json
```

## REST Client (.http files)

For quick API testing in VS Code, use the [REST Client extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client).

### Files

| File | Description |
|------|-------------|
| [api-endpoints.http](api-endpoints.http) | Complete API collection with example requests |

### Usage

1. Install the REST Client extension in VS Code
2. Open `api-endpoints.http`
3. Start the application (`dotnet run --project PoCoupleQuiz.AppHost`)
4. Click "Send Request" above any request block

## API Endpoints

### Health Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health/live` | Kubernetes liveness probe |
| GET | `/health/ready` | Kubernetes readiness probe (checks dependencies) |
| GET | `/api/health` | Detailed health info (version, build date, environment) |

### Teams Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/teams` | List all teams |
| GET | `/api/teams/{name}` | Get team by name |
| POST | `/api/teams` | Create/update team |
| PUT | `/api/teams/{name}/stats` | Update team statistics |

### Questions Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/questions/generate` | Generate AI question |
| POST | `/api/questions/check-similarity` | Check answer similarity |

### Game History Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/game-history` | Save game history |
| GET | `/api/game-history/{teamName}` | Get team's game history |
| GET | `/api/game-history/recent` | Get recent games |

## Authentication

Currently, the API does not require authentication. Future versions may implement Azure AD B2C for user authentication.

## Rate Limiting

No rate limiting is currently implemented. The Azure OpenAI calls are subject to the configured Azure OpenAI resource limits.

## Error Responses

All endpoints return standard HTTP status codes:

| Code | Description |
|------|-------------|
| 200 | Success |
| 400 | Bad Request (validation error) |
| 404 | Not Found |
| 500 | Internal Server Error |

Error responses include a JSON body with details:

```json
{
    "error": "Error description",
    "details": "Additional context"
}
```
