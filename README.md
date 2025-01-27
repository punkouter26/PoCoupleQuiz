# Know Your Friend Quiz

A fun quiz game where players try to match the king player's answers! Built with Blazor Server and Azure services.

## Features

- Real-time quiz game with one king player and 1-6 regular players
- AI-generated questions about the king player
- AI-powered answer similarity checking
- Score tracking and leaderboard
- Game history and statistics
- Beautiful, responsive UI with MudBlazor

## Prerequisites

- .NET 8.0 SDK
- Azure subscription
- Azure OpenAI service instance
- Azure Storage account

## Setup

1. Clone the repository
2. Update the `appsettings.json` file in the `PoCoupleQuiz.Web` project with your Azure service credentials:
   ```json
   {
     "AzureOpenAI": {
       "Endpoint": "https://your-resource-name.openai.azure.com/",
       "Key": "your-azure-openai-key",
       "DeploymentName": "your-deployment-name"
     },
     "AzureStorage": {
       "ConnectionString": "your-storage-account-connection-string"
     }
   }
   ```

3. Run the application:
   ```bash
   cd PoCoupleQuiz.Web
   dotnet run
   ```

4. Open your browser and navigate to `https://localhost:5001`

## How to Play

1. Game Setup:
   - Choose the number of players (1-6 regular players)
   - Enter the king player's name
   - Enter names for all regular players

2. Gameplay:
   - Each round starts with the king player answering a question about themselves
   - All other players then try to match the king's answer
   - Points are awarded for matching answers
   - Real-time scoreboard shows current standings
   - Players can see who has answered each round

3. Game End:
   - Game ends after 5 rounds
   - Final scores are displayed for all players
   - The player with the most matching answers wins!
   - Statistics are saved for the leaderboard

## Architecture

- **PoCoupleQuiz.Web**: Blazor Server application with MudBlazor UI
- **PoCoupleQuiz.Core**: Core business logic and services
- **PoCoupleQuiz.Tests**: Unit tests

### Azure Services Used

- **Azure OpenAI**: Generates questions and checks answer similarity
- **Azure Table Storage**: Stores player information, game history, and statistics

### Features

- **Game Modes**: King player mode where one player answers and others try to match
- **Player Management**: Support for 2-7 total players (1 king + 1-6 regular players)
- **Scoring System**: Points awarded for matching the king's answers
- **Statistics**: Track player performance and game history
- **Leaderboard**: See top performing players
- **Real-time Updates**: Live scoreboard and player status during gameplay

## Development

To run the tests:
```bash
dotnet test
```

## License

MIT 