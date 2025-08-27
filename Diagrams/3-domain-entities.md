```mermaid
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
```
