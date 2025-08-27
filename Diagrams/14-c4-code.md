```mermaid
graph TB
    subgraph "C4 Code Diagram - Game Service Implementation"
        
        subgraph "GameService Class"
            CreateGame[CreateGame Method<br/><br/>Parameters: players, difficulty<br/>Returns: Game object<br/>Validates player count,<br/>initializes game state]
            
            AddPlayer[AddPlayer Method<br/><br/>Parameters: gameId, player<br/>Returns: updated Game<br/>Validates player data,<br/>assigns king if first player]
            
            SubmitAnswer[SubmitAnswer Method<br/><br/>Parameters: gameId, playerId, answer<br/>Returns: GameState<br/>Stores answer, checks completion,<br/>calculates scores if round complete]
            
            CalculateScores[CalculateScores Method<br/><br/>Parameters: gameQuestion<br/>Returns: Dictionary<string, int><br/>Compares answers with king,<br/>awards points for matches]
            
            GetGameStatus[GetGameStatus Method<br/><br/>Parameters: gameId<br/>Returns: GameStatus<br/>Returns current game state,<br/>scores, and progress]
            
            AdvanceRound[AdvanceRound Method<br/><br/>Parameters: gameId<br/>Returns: bool<br/>Moves to next round,<br/>rotates king player]
        end
        
        subgraph "Dependencies"
            IGameRepo[IGameRepository Interface<br/><br/>GetGameAsync(gameId)<br/>SaveGameAsync(game)<br/>GetGameHistoryAsync(playerId)]
            
            IQuestionRepo[IQuestionRepository Interface<br/><br/>GetRandomQuestionsAsync(count, difficulty)<br/>GetQuestionsByCategoryAsync(category)]
            
            ILogger[ILogger Interface<br/><br/>LogInformation(message, args)<br/>LogError(exception, message)<br/>LogWarning(message)]
            
            ICache[ICacheService Interface<br/><br/>GetAsync<T>(key)<br/>SetAsync<T>(key, value, expiry)<br/>RemoveAsync(key)]
        end
        
        subgraph "Domain Models"
            Game[Game Entity<br/><br/>Properties:<br/>- Players: List<Player><br/>- Questions: List<GameQuestion><br/>- CurrentRound: int<br/>- Difficulty: DifficultyLevel<br/><br/>Methods:<br/>- AddPlayer(player)<br/>- GetScoreboard()<br/>- IsGameOver]
            
            Player[Player Entity<br/><br/>Properties:<br/>- Name: string<br/>- Score: int<br/>- IsKingPlayer: bool<br/>- TotalGamesPlayed: int<br/><br/>Methods:<br/>- UpdateScore(points)<br/>- CalculateAccuracy()]
            
            GameQuestion[GameQuestion Entity<br/><br/>Properties:<br/>- Question: Question<br/>- KingAnswer: string<br/>- PlayerAnswers: Dictionary<br/>- Results: Dictionary<br/><br/>Methods:<br/>- AddAnswer(playerId, answer)<br/>- IsCompleted()]
        end
        
        subgraph "Value Objects"
            GameStatus[GameStatus Value Object<br/><br/>Properties:<br/>- IsActive: bool<br/>- CurrentRound: int<br/>- Scores: Dictionary<br/>- Progress: double]
            
            DifficultyLevel[DifficultyLevel Enum<br/><br/>Values:<br/>- Easy = 3 rounds<br/>- Medium = 5 rounds<br/>- Hard = 7 rounds]
        end
        
    end
    
    %% Method Dependencies
    CreateGame -->|Uses| IGameRepo
    CreateGame -->|Uses| IQuestionRepo
    CreateGame -->|Uses| ILogger
    CreateGame -->|Creates| Game
    
    AddPlayer -->|Uses| IGameRepo
    AddPlayer -->|Uses| ILogger
    AddPlayer -->|Modifies| Game
    AddPlayer -->|Adds| Player
    
    SubmitAnswer -->|Uses| IGameRepo
    SubmitAnswer -->|Uses| ICache
    SubmitAnswer -->|Uses| ILogger
    SubmitAnswer -->|Calls| CalculateScores
    SubmitAnswer -->|Updates| GameQuestion
    
    CalculateScores -->|Reads| GameQuestion
    CalculateScores -->|Updates| Player
    CalculateScores -->|Uses| ILogger
    
    GetGameStatus -->|Uses| IGameRepo
    GetGameStatus -->|Uses| ICache
    GetGameStatus -->|Returns| GameStatus
    
    AdvanceRound -->|Uses| IGameRepo
    AdvanceRound -->|Uses| ILogger
    AdvanceRound -->|Modifies| Game
    AdvanceRound -->|Checks| DifficultyLevel
    
    %% Styling
    classDef method fill:#1168bd,color:#ffffff
    classDef interface fill:#2e8b57,color:#ffffff
    classDef entity fill:#ffd700,color:#000000
    classDef valueObject fill:#ff6b6b,color:#ffffff
    
    class CreateGame,AddPlayer,SubmitAnswer,CalculateScores,GetGameStatus,AdvanceRound method
    class IGameRepo,IQuestionRepo,ILogger,ICache interface
    class Game,Player,GameQuestion entity
    class GameStatus,DifficultyLevel valueObject
```
