# E2E Tests Setup

This directory contains End-to-End tests for PoCoupleQuiz using Playwright with TypeScript.

## Prerequisites

- Node.js 18+ installed
- .NET 9.0 SDK installed
- PoCoupleQuiz.Server running on https://localhost:5001

## Installation

```powershell
cd e2e-tests
npm install
npx playwright install chromium
```

## Running Tests

```powershell
# Run all tests headless
npm test

# Run tests with browser visible
npm run test:headed

# Debug tests interactively
npm run test:debug

# View HTML report
npm run test:report
```

## Test Configuration

- **Browser**: Chromium only (per project requirements)
- **Desktop Viewport**: 1920x1080
- **Mobile Viewport**: 375x667 (iPhone SE portrait)
- **Base URL**: https://localhost:5001
- **HTTPS**: Self-signed certificate errors ignored for local development

## Test Coverage

### Home Page Tests
- Page loads and displays title
- Navigation menu visible
- Responsive layout on mobile

### Game Flow Tests
- Start new game functionality
- Question display
- Timer component existence

### Leaderboard Tests
- Leaderboard page navigation
- Team statistics display
- Scoreboard component rendering

### Diagnostics Page Tests
- /diag page loads correctly
- Health check status displays
- Refresh functionality works
- Mobile responsive design

### Responsive Design Tests
- Readable text on mobile (>= 14px font)
- Touch-friendly controls (>= 40px height)
- No horizontal scroll on mobile

## Notes

- Tests run **locally only** (not part of CI/CD pipeline)
- The web server starts automatically before tests run
- Blazor WebAssembly requires `networkidle` wait state for proper loading
- Some tests use conditional logic to handle optional UI elements
