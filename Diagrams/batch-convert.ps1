$diagrams = @(
    @{
        name = "3-domain-entities"
        content = @"
classDiagram
    class Game {
        +List~Player~ Players
        +int CurrentKingPlayerIndex
        +Player KingPlayer
        +List~GameQuestion~ Questions
        +int CurrentRound
        +DifficultyLevel Difficulty
        +int MaxRounds
        +bool IsGameOver
        +int MinimumPlayers
        +bool HasEnoughPlayers
        +bool HasKingPlayer
        +Dictionary~string,int~ GetScoreboard()
        +void AddPlayer(Player player)
    }
    
    class Player {
        +string Name
        +int Score
        +bool IsKingPlayer
        +int TotalGamesPlayed
        +int TotalCorrectGuesses
        +double GuessAccuracy
    }
    
    class Question {
        +string Text
        +QuestionCategory Category
        +DateTime CreatedAt
    }
    
    class GameQuestion {
        +Question Question
        +string KingAnswer
        +Dictionary~string,string~ PlayerAnswers
        +Dictionary~string,bool~ Results
        +bool IsCompleted
    }
    
    class GameHistory {
        +string GameId
        +DateTime StartTime
        +DateTime EndTime
        +List~Player~ Players
        +DifficultyLevel Difficulty
        +Dictionary~string,int~ FinalScores
        +int TotalRounds
        +TimeSpan Duration
    }
    
    class Team {
        +string Name
        +List~Player~ Members
        +int TotalScore
        +DateTime CreatedAt
    }
    
    class GameMode {
        +string Name
        +string Description
        +int MaxPlayers
        +int MinPlayers
        +TimeSpan TimeLimit
        +bool IsTeamBased
    }
    
    %% Enums
    class DifficultyLevel {
        <<enumeration>>
        Easy
        Medium
        Hard
    }
    
    class QuestionCategory {
        <<enumeration>>
        Relationships
        Hobbies
        Childhood
        Future
        Preferences
        Values
    }
    
    %% Relationships
    Game ||--o{ Player : contains
    Game ||--o{ GameQuestion : contains
    Game ||-- DifficultyLevel : has
    GameQuestion ||-- Question : references
    Question ||-- QuestionCategory : categorized
    GameHistory ||--o{ Player : records
    GameHistory ||-- DifficultyLevel : played
    Team ||--o{ Player : groups
    GameMode ||-- DifficultyLevel : supports
"@
    },
    @{
        name = "4-api-sequence"
        content = @"
sequenceDiagram
    participant C as Blazor Client
    participant API as Web API
    participant GS as Game Service
    participant QS as Question Service
    participant DB as Azure Table Storage
    
    Note over C,DB: Start New Game Flow
    
    C->>+API: POST /api/game/start
    Note right of C: {players, difficulty}
    
    API->>+GS: CreateNewGame(players, difficulty)
    GS->>GS: Validate players (min 2)
    GS->>GS: Set first player as King
    
    GS->>+QS: GetQuestionsForDifficulty(difficulty)
    QS->>+DB: Query Questions table
    DB-->>-QS: Questions collection
    QS->>QS: Filter by category
    QS->>QS: Randomize selection
    QS-->>-GS: Selected questions
    
    GS->>GS: Create Game object
    GS->>+DB: Insert Game record
    DB-->>-GS: Success/GameId
    GS-->>-API: Game object
    
    API-->>-C: 201 Created {gameId, questions}
    
    Note over C,DB: Submit Answers Flow
    
    C->>+API: POST /api/game/{gameId}/answers
    Note right of C: {playerId, answers}
    
    API->>+GS: SubmitAnswers(gameId, playerId, answers)
    GS->>+DB: Get Game record
    DB-->>-GS: Game object
    
    GS->>GS: Validate game state
    GS->>GS: Store player answers
    GS->>GS: Check if all players answered
    
    alt All players answered
        GS->>GS: Calculate scores
        GS->>GS: Update player scores
        GS->>+DB: Update Game record
        DB-->>-GS: Success
        GS-->>-API: Game with updated scores
        API-->>-C: 200 OK {game, scores}
    else Waiting for more answers
        GS->>+DB: Update Game record
        DB-->>-GS: Success
        GS-->>-API: Waiting status
        API-->>-C: 202 Accepted {waiting}
    end
    
    Note over C,DB: Get Game Status Flow
    
    C->>+API: GET /api/game/{gameId}/status
    API->>+GS: GetGameStatus(gameId)
    GS->>+DB: Get Game record
    DB-->>-GS: Game object
    GS->>GS: Calculate progress
    GS-->>-API: Game status
    API-->>-C: 200 OK {status, scores, progress}
"@
    }
)

$diagramsPath = "c:\Users\punko\Downloads\PoCoupleQuiz\Diagrams"

foreach ($diagram in $diagrams) {
    $mmdFile = Join-Path $diagramsPath "$($diagram.name).mmd"
    $svgFile = Join-Path $diagramsPath "$($diagram.name).svg"
    
    Write-Host "Creating $($diagram.name).mmd..." -ForegroundColor Yellow
    $diagram.content | Out-File -FilePath $mmdFile -Encoding UTF8
    
    Write-Host "Converting to SVG..." -ForegroundColor Yellow
    $command = "mmdc -i `"$mmdFile`" -o `"$svgFile`" --backgroundColor transparent"
    try {
        Invoke-Expression $command
        Write-Host "✓ Created $($diagram.name).svg" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Failed to convert $($diagram.name)" -ForegroundColor Red
    }
}
