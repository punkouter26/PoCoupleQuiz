# PoCoupleQuiz 💕

A fun, interactive web-based quiz application where couples and friends can test how well they know each other! One player acts as the "King Player" whose answers become the reference, while others try to guess their responses to various personal questions.

## 🎯 Project Summary

PoCoupleQuiz is built with modern web technologies using a clean architecture approach:

- **Frontend**: Blazor WebAssembly for rich, interactive UI
- **Backend**: .NET 9 Web API with clean architecture principles  
- **Database**: Azure Table Storage for cloud persistence
- **Architecture**: Onion Architecture with clear separation of concerns
- **Testing**: Comprehensive unit and integration tests with XUnit
- **Deployment**: Azure App Service with automated CI/CD

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio 2022](https://visualstudio.microsoft.com/)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (for deployment)
- [Node.js](https://nodejs.org/) (for local development tools)

### Step 1: Clone the Repository

```bash
git clone https://github.com/punkouter25/PoCoupleQuiz.git
cd PoCoupleQuiz
```

### Step 2: Install Azurite (Local Development)

Install Azurite globally for local Azure Table Storage emulation:

```bash
npm install -g azurite
```

### Step 3: Start Azurite

Open a terminal and start the Azurite storage emulator:

```bash
azurite --location AzuriteData --debug azurite-debug.log
```

Leave this terminal running during development.

### Step 4: Restore Dependencies

```bash
dotnet restore
```

### Step 5: Build the Solution

```bash
dotnet build
```

### Step 6: Run Tests (Optional)

Verify everything is working by running the test suite:

```bash
dotnet test
```

### Step 7: Start the Application

You can run the application using either of these methods:

#### Option A: Using VS Code Tasks
1. Open the project in VS Code
2. Press `Ctrl+Shift+P` and select "Tasks: Run Task"
3. Choose "watch" to start the development server with hot reload

#### Option B: Using Terminal
```bash
cd PoCoupleQuiz.Server
dotnet run
```

### Step 8: Access the Application

Open your browser and navigate to:
- **HTTPS**: https://localhost:7154
- **HTTP**: http://localhost:5154

## 🎮 How to Play

1. **Setup**: Enter player names and select one as the "King Player"
2. **Choose Difficulty**: Easy (3 rounds), Medium (5 rounds), or Hard (7 rounds)
3. **Gameplay**: 
   - King Player answers questions privately
   - Other players guess what the King Player answered
   - Points awarded for correct guesses
4. **Scoring**: View live scoreboard and track statistics over time

## 📁 Project Structure

```
PoCoupleQuiz/
├── PoCoupleQuiz.Core/          # Domain models and business logic
├── PoCoupleQuiz.Server/        # Web API backend
├── PoCoupleQuiz.Client/        # Blazor WebAssembly frontend
├── PoCoupleQuiz.Tests/         # Unit and integration tests
├── Diagrams/                   # Architecture and design diagrams
├── AzuriteData/               # Local storage emulator data
└── .github/workflows/         # CI/CD pipeline definitions
```

## 🔧 Development

### Key Technologies
- **.NET 9**: Latest framework features
- **Blazor WebAssembly**: Client-side C# development
- **Azure Table Storage**: NoSQL cloud database
- **XUnit**: Testing framework
- **Serilog**: Structured logging
- **Clean Architecture**: Maintainable, testable code organization

### Development Workflow
1. Make changes to the code
2. Tests run automatically (if using watch task)
3. Use the diagnostics page (`/diag`) to verify system health
4. Check logs in `log.txt` for debugging information

### Available Endpoints
- **Game API**: `/api/game/*` - Game management and state
- **Questions API**: `/api/questions/*` - Question retrieval and management
- **Diagnostics API**: `/api/diag/*` - Health checks and system status

## 🚀 Deployment

### Local Deployment
The application runs locally using Azurite for storage emulation.

### Azure Deployment
1. Ensure Azure CLI is installed and authenticated
2. Run deployment script:
   ```bash
   azd up
   ```
3. Follow prompts for resource group and region selection

### Environment Configuration
- **Local**: Uses `appsettings.Development.json`
- **Azure**: Uses Azure App Service configuration and Key Vault

## 📊 Monitoring

- **Application Insights**: Performance and error tracking
- **Structured Logging**: Detailed logs in `log.txt`
- **Health Checks**: Available via `/diag` endpoint
- **Azure Monitor**: Cloud-based monitoring and alerting

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙋 Support

For questions or issues:
1. Check the diagnostics page (`/diag`) for system status
2. Review logs in `log.txt`
3. Create an issue in the GitHub repository
4. Contact the development team

---

**Happy Quizzing!** 🎉
