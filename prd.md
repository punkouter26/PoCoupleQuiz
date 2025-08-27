# PoCoupleQuiz - Product Requirements Document

## Application Overview

**PoCoupleQuiz** is an interactive web-based quiz application designed for couples and friends to test how well they know each other. The application features a "King Player" system where one player acts as the answer reference while others guess their responses to various relationship and personal questions.

### Key Features
- **Multi-player Support**: Supports 2+ players with one designated "King Player"
- **Dynamic Question Categories**: Questions spanning relationships, hobbies, childhood, future, preferences, and values
- **Difficulty Levels**: Easy (3 rounds), Medium (5 rounds), Hard (7 rounds)
- **Real-time Scoring**: Live scoreboard with player rankings
- **Game Statistics**: Track accuracy, games played, and performance over time
- **Responsive Design**: Optimized for desktop and mobile devices
- **Azure Integration**: Cloud-hosted with Azure Table Storage for data persistence

### Architecture
- **Frontend**: Blazor WebAssembly for interactive UI
- **Backend**: .NET 9 Web API with clean architecture principles
- **Database**: Azure Table Storage (Azurite for local development)
- **Deployment**: Azure App Service with CI/CD via GitHub Actions

## UI Pages & Components

### Core Pages

#### 1. Home Page (`Index.razor`)
- **Purpose**: Main landing page and game setup
- **Components**:
  - Welcome message and game description
  - Player registration form
  - Difficulty selection
  - "Start Game" button
  - Navigation to other pages

#### 2. Game Page (`Game.razor`)
- **Purpose**: Main gameplay interface
- **Components**:
  - Current question display
  - King player indicator
  - Answer input forms for guessing players
  - Round progress indicator
  - Real-time scoring updates
  - Timer component for each round
  - Submit answers functionality

#### 3. Leaderboard Page (`Leaderboard.razor`)
- **Purpose**: Display current game scores and rankings
- **Components**:
  - ScoreboardDisplay component
  - Player rankings table
  - Current round information
  - Game progress visualization

#### 4. Statistics Page (`Statistics.razor`)
- **Purpose**: Historical game data and player analytics
- **Components**:
  - Player accuracy statistics
  - Games played history
  - Performance trends
  - Category-based analysis

#### 5. Diagnostics Page (`Diag.razor`)
- **Purpose**: System health monitoring and troubleshooting
- **Components**:
  - Azure Table Storage connection status
  - API health checks
  - Internet connectivity verification
  - Performance metrics

### Shared Components

#### 1. MainLayout (`MainLayout.razor`)
- **Purpose**: Application shell and navigation
- **Features**:
  - Responsive navigation menu
  - Consistent header/footer
  - Mobile-friendly hamburger menu

#### 2. ScoreboardDisplay (`ScoreboardDisplay.razor`)
- **Purpose**: Reusable scoring component
- **Features**:
  - Player score visualization
  - Ranking indicators
  - Real-time updates

#### 3. GameTimer (`GameTimer.razor`)
- **Purpose**: Round timing functionality
- **Features**:
  - Countdown timer
  - Visual progress indicator
  - Auto-submission on timeout

#### 4. LoadingState (`LoadingState.razor`)
- **Purpose**: Loading indicators and state management
- **Features**:
  - Spinner animations
  - Progress messages
  - Error handling

#### 5. NavMenu (`NavMenu.razor`)
- **Purpose**: Application navigation
- **Features**:
  - Route-based highlighting
  - Responsive design
  - Quick access to all pages

#### 6. AppErrorBoundary (`AppErrorBoundary.razor`)
- **Purpose**: Global error handling
- **Features**:
  - Graceful error display
  - Error reporting
  - Recovery options

### Technical Requirements
- **Responsive Design**: Must work on screens 320px and larger
- **Performance**: Page load times under 3 seconds
- **Accessibility**: WCAG 2.1 AA compliance
- **Browser Support**: Chrome, Firefox, Safari, Edge (latest 2 versions)
- **Real-time Updates**: WebSocket or SignalR integration for live scoring
- **Offline Capability**: Service worker for basic offline functionality
