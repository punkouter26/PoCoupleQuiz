# Features Documentation

## Overview

PoCoupleQuiz is an interactive multiplayer quiz game. This document catalogs all features with their implementation status and technical details.

---

## Core Features

### ✅ Multi-Player Support

| Aspect | Details |
|--------|---------|
| **Status** | Implemented |
| **Min Players** | 2 |
| **Max Players** | Unlimited (practical limit ~8) |
| **Implementation** | `Game.Players` list in `Core/Models/Game.cs` |

**How it works:**
- Players added during game setup on Home page
- Each player takes turns as "King Player"
- Other players are "Guessing Players" for that round

---

### ✅ King Player Mechanics

| Aspect | Details |
|--------|---------|
| **Status** | Implemented |
| **Files** | `GameStateService.cs`, `Game.razor` |

**How it works:**
1. First player starts as King
2. King sees question and answers truthfully about themselves
3. Other players try to guess King's answer
4. After round, next player becomes King
5. Rotates through all players

---

### ✅ AI-Powered Question Generation

| Aspect | Details |
|--------|---------|
| **Status** | Implemented |
| **Provider** | Azure OpenAI (GPT-4o) |
| **Fallback** | MockQuestionService (local dev) |
| **Files** | `AzureOpenAIQuestionService.cs`, `MockQuestionService.cs` |

**Categories:**
- Relationships
- Hobbies
- Childhood
- Future
- Preferences
- Values

**Example prompt structure:**
```
Generate a fun relationship quiz question suitable for couples or friends.
Difficulty: {difficulty}
Format: Return only the question text.
```

---

### ✅ Semantic Answer Matching

| Aspect | Details |
|--------|---------|
| **Status** | Implemented |
| **Provider** | Azure OpenAI |
| **Files** | `AzureOpenAIQuestionService.CheckAnswerSimilarityAsync()` |

**How it works:**
- AI compares King's answer with each guess
- Semantic matching (not exact string match)
- "pizza" matches "Italian pizza with pepperoni"
- Returns true/false for each comparison

---

### ✅ Difficulty Levels

| Level | Rounds | Answer Time |
|-------|--------|-------------|
| Easy | 3 | 40 seconds |
| Medium | 5 | 30 seconds |
| Hard | 7 | 20 seconds |

**Implementation:** `Difficulty` enum in `Core/Models/Difficulty.cs`

---

### ✅ Real-Time Scoreboard

| Aspect | Details |
|--------|---------|
| **Status** | Implemented |
| **Files** | `ScoreboardDisplay.razor` |

**Features:**
- Live score updates with animations
- Rank emojis (🥇🥈🥉)
- Progress bars showing relative scores
- +Points indicators when score increases
- Haptic feedback on mobile (score increase)

---

### ✅ Global Leaderboard

| Aspect | Details |
|--------|---------|
| **Status** | Implemented |
| **Files** | `Leaderboard.razor`, `TeamsController.cs` |
| **Storage** | Azure Table Storage |

**Features:**
- Top 10 players by accuracy
- Accuracy percentage display
- Total questions answered
- "Last played" time-ago formatting
- Trophy badges for top 3

---

### ✅ Game History Persistence

| Aspect | Details |
|--------|---------|
| **Status** | Implemented |
| **Files** | `GameHistoryController.cs`, `AzureTableGameHistoryService.cs` |

**Tracked data:**
- Team names
- Scores per team
- Total questions
- Game mode/difficulty
- Timestamp

---

## UI/UX Features

### ✅ Responsive Design

| Aspect | Details |
|--------|---------|
| **Status** | Implemented |
| **Breakpoint** | 640px (mobile) |
| **Approach** | Mobile-first CSS |

**Mobile adaptations:**
- Collapsed stats on leaderboard
- Touch-friendly buttons
- Simplified layouts

---

### ✅ Modern 2026 Design System

| Aspect | Details |
|--------|---------|
| **Status** | Implemented (Feb 2026) |
| **Files** | `app.css` |

**Design tokens:**
```css
--primary: #6366f1 (Indigo)
--accent: #ec4899 (Pink)
--gradient-primary: linear-gradient(135deg, #6366f1, #8b5cf6)
```

**Animations:**
- `pageSlideIn` - Page transitions
- `fadeSlide` - Component reveals
- `matchCelebrate` - Correct answer celebration
- `mismatchShake` - Incorrect answer shake

---

### ✅ Skeleton Loaders

| Aspect | Details |
|--------|---------|
| **Status** | Implemented |
| **Files** | `SkeletonLoader.razor` |

**Types:**
- `question` - Question loading placeholder
- `leaderboard` - Table loading placeholder
- `game` - Game state loading

---

### ✅ Error Boundaries

| Aspect | Details |
|--------|---------|
| **Status** | Implemented |
| **Files** | `AppErrorBoundary.razor` |

**Features:**
- Catches unhandled exceptions
- Shows friendly error message
- Provides recovery button
- Logs errors to Application Insights

---

## Technical Features

### ✅ OpenTelemetry Observability

| Signal | Implementation |
|--------|----------------|
| Logs | Serilog → App Insights |
| Metrics | Custom metrics (game duration, question latency) |
| Traces | Distributed tracing across services |

**Custom metrics:**
- `game.duration` - Total game time
- `question.latency` - AI response time
- `active.players` - Concurrent players

---

### ✅ Health Checks

| Endpoint | Purpose |
|----------|---------|
| `/health/live` | Kubernetes liveness |
| `/health/ready` | Kubernetes readiness |
| `/api/health` | Detailed status + version |

**Ready check includes:**
- Azure Table Storage connectivity
- Azure OpenAI availability (optional)

---

### ✅ Central Package Management

| Aspect | Details |
|--------|---------|
| **Status** | Implemented |
| **Files** | `Directory.Packages.props` |

All NuGet versions managed centrally for consistency.

---

### ✅ CI/CD Pipeline

| Aspect | Details |
|--------|---------|
| **Platform** | GitHub Actions |
| **Trigger** | Push to `master` |
| **Target** | Azure Container Apps |

**Pipeline steps:**
1. Build & test
2. Push container to ACR
3. Deploy via `azd`

---

## Planned Features

### 🔲 User Authentication

| Aspect | Details |
|--------|---------|
| **Status** | Not implemented |
| **Planned** | Azure AD B2C |
| **Priority** | Medium |

---

### 🔲 Real-Time Multiplayer (SignalR)

| Aspect | Details |
|--------|---------|
| **Status** | Not implemented |
| **Planned** | SignalR hub for live updates |
| **Priority** | Low |

Currently uses polling/client-side state.

---

### 🔲 Custom Question Packs

| Aspect | Details |
|--------|---------|
| **Status** | Not implemented |
| **Priority** | Low |

Allow users to create/share question sets.

---

## Feature Matrix

| Feature | Client | Server | Core | Azure |
|---------|--------|--------|------|-------|
| Player Management | ✅ | ✅ | ✅ | Table Storage |
| Question Generation | ✅ | ✅ | ✅ | OpenAI |
| Answer Matching | ✅ | ✅ | ✅ | OpenAI |
| Scoring | ✅ | ✅ | ✅ | - |
| Leaderboard | ✅ | ✅ | ✅ | Table Storage |
| Game History | ✅ | ✅ | ✅ | Table Storage |
| Telemetry | ✅ | ✅ | ✅ | App Insights |
| Health Checks | - | ✅ | ✅ | - |
