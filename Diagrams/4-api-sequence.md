```mermaid
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
```
