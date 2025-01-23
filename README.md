# Couple Quiz Game

A fun quiz game for couples to test how well they know each other! Built with Blazor Server and Azure services.

## Features

- Real-time quiz game for two couples
- AI-generated questions about relationships
- AI-powered answer similarity checking
- Score tracking and persistence
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

1. Enter team names and partner names for both couples
2. Each round:
   - Partner A answers a question about Partner B
   - Partner B then tries to match Partner A's answer
   - Points are awarded for matching answers
3. After 3 rounds, the team with the most points wins!

## Architecture

- **PoCoupleQuiz.Web**: Blazor Server application with MudBlazor UI
- **PoCoupleQuiz.Core**: Core business logic and services
- **PoCoupleQuiz.Tests**: Unit tests

### Azure Services Used

- **Azure OpenAI**: Generates questions and checks answer similarity
- **Azure Table Storage**: Stores team information and game statistics

## Development

To run the tests:
```bash
dotnet test
```

## License

MIT 