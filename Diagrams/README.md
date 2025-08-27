# PoCoupleQuiz - Architecture Diagrams

This folder contains comprehensive architectural and design diagrams for the PoCoupleQuiz application. Each diagram is available in both Mermaid markdown format (`.md`) and SVG format (`.svg`).

## Diagram Overview

### 1. Onion Architecture Layer Diagram
**File**: `1-onion-architecture.md/svg`

Shows the high-level architectural layers and their dependency rules:
- **Domain Layer (Core)**: Business entities, value objects, and domain services
- **Application Layer**: Application services, interfaces, and validation logic
- **Infrastructure Layer**: Data persistence, external APIs, and file systems
- **Presentation Layer**: Blazor UI and Web API controllers

### 2. Project Dependency Diagram
**File**: `2-project-dependencies.md/svg`

Visualizes the .NET project structure and dependencies:
- PoCoupleQuiz.Core (Domain models)
- PoCoupleQuiz.Server (Web API)
- PoCoupleQuiz.Client (Blazor WebAssembly)
- PoCoupleQuiz.Tests (Testing project)
- External dependencies (Azure, browsers)

### 3. Class Diagram for Domain Entities
**File**: `3-domain-entities.md/svg`

Models the core business objects and their relationships:
- Game, Player, Question entities
- GameQuestion, GameHistory, Team objects
- Enumerations (DifficultyLevel, QuestionCategory)
- Properties, methods, and associations

### 4. Sequence Diagram for API Calls
**File**: `4-api-sequence.md/svg`

Traces API interactions for key workflows:
- Start new game flow
- Submit answers process
- Get game status requests
- Error handling scenarios

### 5. Blazor Component Interaction Diagram
**File**: `5-blazor-component-interaction.md/svg`

Illustrates component communication patterns:
- Parameter passing between components
- Event handling and state management
- Service injection and dependencies
- Component lifecycle interactions

### 6. State Diagram for Blazor Components
**File**: `6-component-state.md/svg`

Represents component lifecycle states:
- Initializing → Loading → Ready states
- Error handling and retry logic
- Updating and refreshing flows
- Disposal and cleanup processes

### 7. Entity Relationship Diagram (ERD)
**File**: `7-entity-relationship.md/svg`

Displays the Azure Table Storage schema:
- Table structures and partition/row keys
- Entity relationships and constraints
- Data types and required fields
- Foreign key relationships

### 8. Flowchart for Gameplay Use Case
**File**: `8-gameplay-flowchart.md/svg`

Outlines the complete game flow:
- Player setup and validation
- Question loading and display
- Answer submission and scoring
- Round progression and completion
- Error handling and recovery

### 9. Component Hierarchy Diagram
**File**: `9-component-hierarchy.md/svg`

Tree-like view of Blazor component structure:
- Page-level components
- Shared components and their usage
- Component composition patterns
- Reusable component identification

### 10. Infrastructure Diagram
**File**: `10-infrastructure.md/svg`

Visualizes the deployment environment:
- Azure cloud services integration
- Development and production environments
- External service dependencies
- Data flow and security boundaries

## C4 Model Diagrams

### 11. C4 Context Diagram
**File**: `11-c4-context.md/svg`

System context showing:
- External users (couples, administrators)
- System boundaries
- External systems and their purposes
- High-level relationships

### 12. C4 Container Diagram
**File**: `12-c4-container.md/svg`

Application containers:
- Blazor WebAssembly client
- ASP.NET Core Web API
- Azure Table Storage
- Monitoring and logging services

### 13. C4 Component Diagram
**File**: `13-c4-component.md/svg`

Web API internal components:
- Controllers (Game, Question, Diagnostics)
- Services (Business logic layer)
- Repositories (Data access layer)
- Domain models and infrastructure

### 14. C4 Code Diagram
**File**: `14-c4-code.md/svg`

Detailed code-level view:
- GameService class implementation
- Method signatures and responsibilities
- Interface dependencies
- Domain model interactions

## Generating SVG Files

To convert Mermaid diagrams to SVG format:

1. Install mermaid-cli globally:
   ```bash
   npm install -g @mermaid-js/mermaid-cli
   ```

2. Run the conversion script:
   ```powershell
   .\convert-to-svg.ps1
   ```

## Usage Guidelines

### For Developers
- Reference architecture diagrams during development
- Use component diagrams for understanding system structure
- Consult sequence diagrams for API implementation
- Follow C4 model for system documentation

### For Stakeholders
- Start with C4 Context diagram for system overview
- Review infrastructure diagram for deployment understanding
- Use flowchart for business process comprehension
- Reference ERD for data structure clarity

### For Documentation
- Include relevant diagrams in design documents
- Use SVG format for web-based documentation
- Reference specific diagrams in code comments
- Keep diagrams updated with system changes

## Maintenance

These diagrams should be updated when:
- New features are added
- Architecture patterns change
- Database schema evolves
- Deployment infrastructure changes
- Component relationships modify

## Tools Used

- **Mermaid**: Diagram creation and maintenance
- **Mermaid CLI**: SVG conversion
- **Visual Studio Code**: Editing with Mermaid preview extensions
- **C4 Model**: Architectural documentation methodology
