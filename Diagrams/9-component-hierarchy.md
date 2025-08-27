```mermaid
graph TD
    subgraph "Page Hierarchy"
        App[App.razor<br/>Application Root]
        
        subgraph "Layout Layer"
            MainLayout[MainLayout.razor<br/>Primary Layout]
        end
        
        subgraph "Page Components"
            Index[Index.razor<br/>@page "/"]
            Game[Game.razor<br/>@page "/game"]
            Leaderboard[Leaderboard.razor<br/>@page "/leaderboard"]
            Statistics[Statistics.razor<br/>@page "/statistics"]
            Diag[Diag.razor<br/>@page "/diag"]
        end
        
        subgraph "Shared Components - Navigation"
            NavMenu[NavMenu.razor<br/>Navigation Menu]
        end
        
        subgraph "Shared Components - Game"
            ScoreDisplay[ScoreboardDisplay.razor<br/>Score Visualization]
            GameTimer[GameTimer.razor<br/>Round Timer]
            LoadingState[LoadingState.razor<br/>Loading Indicators]
        end
        
        subgraph "Shared Components - Infrastructure"
            ErrorBoundary[AppErrorBoundary.razor<br/>Error Handling]
        end
        
        subgraph "Game Page Sub-Components"
            PlayerSetup[Player Setup Form]
            QuestionDisplay[Question Display]
            AnswerInput[Answer Input Forms]
            RoundProgress[Round Progress Indicator]
        end
        
        subgraph "Statistics Page Sub-Components"
            PlayerStats[Individual Player Stats]
            GameTrends[Game Trends Chart]
            CategoryAnalysis[Category Performance]
        end
        
        subgraph "Diagnostics Components"
            ConnectionStatus[Connection Health]
            PerformanceMetrics[Performance Data]
            SystemInfo[System Information]
        end
    end
    
    %% Hierarchy Relationships
    App --> MainLayout
    MainLayout --> Index
    MainLayout --> Game
    MainLayout --> Leaderboard
    MainLayout --> Statistics
    MainLayout --> Diag
    MainLayout --> NavMenu
    
    %% Component Composition
    Game --> PlayerSetup
    Game --> QuestionDisplay
    Game --> AnswerInput
    Game --> RoundProgress
    Game --> ScoreDisplay
    Game --> GameTimer
    Game --> LoadingState
    Game --> ErrorBoundary
    
    Statistics --> PlayerStats
    Statistics --> GameTrends
    Statistics --> CategoryAnalysis
    Statistics --> LoadingState
    Statistics --> ErrorBoundary
    
    Diag --> ConnectionStatus
    Diag --> PerformanceMetrics
    Diag --> SystemInfo
    Diag --> LoadingState
    Diag --> ErrorBoundary
    
    Leaderboard --> ScoreDisplay
    Leaderboard --> LoadingState
    Leaderboard --> ErrorBoundary
    
    Index --> LoadingState
    Index --> ErrorBoundary
    
    %% Cross-Component Usage
    Index -.Reused.-> ScoreDisplay
    Statistics -.Reused.-> ScoreDisplay
    
    %% Styling
    classDef root fill:#e8f5e8
    classDef layout fill:#fff3e0
    classDef page fill:#e3f2fd
    classDef shared fill:#f3e5f5
    classDef subcomp fill:#fafafa
    
    class App root
    class MainLayout layout
    class Index,Game,Leaderboard,Statistics,Diag page
    class NavMenu,ScoreDisplay,GameTimer,LoadingState,ErrorBoundary shared
    class PlayerSetup,QuestionDisplay,AnswerInput,RoundProgress,PlayerStats,GameTrends,CategoryAnalysis,ConnectionStatus,PerformanceMetrics,SystemInfo subcomp
```
