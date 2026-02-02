# PoCoupleQuiz - Product Requirements Document

## Application Overview

**PoCoupleQuiz** is an interactive web-based quiz application designed for couples and friends to test how well they know each other. The application features a "King Player" system where one player acts as the answer reference while others guess their responses to various relationship and personal questions.

### Key Features
- **Multi-player Support**: Supports 2+ players with one designated "King Player"
- **Dynamic Question Categories**: Questions spanning relationships, hobbies, childhood, future, preferences, and values
- **Difficulty Levels**: Easy (3 rounds), Medium (5 rounds), Hard (7 rounds)
- **Real-time Scoring**: Live scoreboard with player rankings and animations
- **Game Statistics**: Track accuracy, games played, and performance over time
- **Responsive Design**: Optimized for desktop and mobile devices with modern 2026 UI
- **Azure Integration**: Cloud-hosted with Azure Table Storage, OpenAI, and Application Insights

### Architecture
- **Frontend**: Blazor WebAssembly with Radzen components
- **Backend**: .NET 10 Web API with Vertical Slice Architecture
- **Database**: Azure Table Storage (Azurite for local development)
- **AI**: Azure OpenAI GPT-4o for question generation and answer matching
- **Deployment**: Azure Container Apps via .NET Aspire 13.1.0

## UI Pages & Components

### Core Pages

#### 1. Home Page (`Index.razor`)
- **Purpose**: Main landing page and game setup
- **Components**:
  - Welcome message with gradient branding
  - Player registration form
  - Difficulty selection (Easy/Medium/Hard)
  - "Start Game" button with hover animations
  - Navigation to Leaderboard

#### 2. Game Page (`Game.razor`)
- **Purpose**: Main gameplay interface
- **Components**:
  - QuestionDisplay - Shows current question with typography styling
  - King player indicator with crown emoji
  - Answer input forms for guessing players
  - Round progress indicator
  - ScoreboardDisplay - Real-time scoring with progress bars
  - RoundResults - Match/mismatch animations with icons

#### 3. Leaderboard Page (`Leaderboard.razor`)
- **Purpose**: Display global player rankings
- **Components**:
  - Hall of Fame header with gradient styling
  - Rank badges (🥇🥈🥉) for top 3
  - Trophy decorations (👑⭐✨)
  - Player cards with accuracy stats
  - Time-ago formatting for last played

#### 4. Diagnostics Page (`Diag.razor`)
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
  - Modern gradient header (sticky positioning)
  - Inline navigation links (Home, Leaderboard)
  - Brand icon with heart emoji
  - No sidebar (removed for cleaner design)

#### 2. QuestionDisplay (`QuestionDisplay.razor`)
- **Purpose**: Display questions with enhanced typography
- **Features**:
  - Gradient text styling
  - Decorative quotation marks (::before/::after)
  - 1.75rem font size for readability
  - Box shadow and rounded corners

#### 3. ScoreboardDisplay (`ScoreboardDisplay.razor`)
- **Purpose**: Real-time scoring visualization
- **Features**:
  - Rank emojis (🥇🥈🥉)
  - Progress bars showing relative scores
  - +Points gained indicators with animations
  - Haptic feedback on mobile (score increase)
  - Score increase animation (scale pulse)

#### 4. RoundResults (`RoundResults.razor`)
- **Purpose**: Show round outcome with feedback
- **Features**:
  - ✅/❌ icons for match/mismatch
  - Match celebration animation (pulse)
  - Mismatch shake animation
  - +10 pts badge for correct matches
  - King's answer highlight (gold theme)

#### 5. SkeletonLoader (`SkeletonLoader.razor`)
- **Purpose**: Loading state placeholders
- **Types**: question, leaderboard, game
- **Features**: Shimmer animation, consistent sizing

#### 6. AppErrorBoundary (`AppErrorBoundary.razor`)
- **Purpose**: Global error handling
- **Features**:
  - Graceful error display
  - Error logging to Application Insights
  - Recovery button

### Technical Requirements
- **Responsive Design**: Must work on screens 320px and larger
- **Performance**: Page load times under 3 seconds
- **Accessibility**: WCAG 2.1 AA compliance goals
- **Browser Support**: Chrome, Firefox, Safari, Edge (latest 2 versions)
- **Animations**: CSS keyframes for state transitions (pageSlideIn, fadeSlide)
- **Color Palette**: Indigo primary (#6366f1), Pink accent (#ec4899)
