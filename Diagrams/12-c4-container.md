```mermaid
graph TB
    subgraph "C4 Container Diagram - PoCoupleQuiz System"
        
        subgraph "Users"
            Users[Game Players<br/>Web Browser Users]
        end
        
        subgraph "PoCoupleQuiz System Boundary"
            
            subgraph "Client Side"
                WebApp[Web Application<br/>Blazor WebAssembly<br/><br/>Provides interactive game UI,<br/>real-time scoring, player<br/>management, and statistics]
            end
            
            subgraph "Server Side"
                API[Web API Application<br/>ASP.NET Core 9<br/><br/>Handles game logic, question<br/>management, scoring calculations,<br/>and data persistence]
            end
            
            subgraph "Data Layer"
                TableDB[Azure Table Storage<br/>NoSQL Database<br/><br/>Stores game data, player<br/>information, questions,<br/>and game history]
                
                BlobStore[Azure Blob Storage<br/>File Storage<br/><br/>Stores static assets,<br/>application logs,<br/>and backup data]
            end
            
        end
        
        subgraph "External Containers"
            Browser[Web Browser<br/>JavaScript Runtime<br/><br/>Executes Blazor WebAssembly<br/>application and provides<br/>user interface]
            
            AppInsights[Application Insights<br/>Monitoring Service<br/><br/>Collects telemetry data,<br/>performance metrics,<br/>and error logs]
            
            LogAnalytics[Log Analytics<br/>Centralized Logging<br/><br/>Aggregates and analyzes<br/>application logs and<br/>system metrics]
        end
    end
    
    %% User Interactions
    Users -->|Accesses game via HTTPS<br/>Views pages, submits answers| WebApp
    
    %% Application Flow
    WebApp -->|Makes API calls via HTTP/JSON<br/>Game operations, data requests| API
    Browser -->|Hosts and executes<br/>Blazor WebAssembly runtime| WebApp
    
    %% Data Persistence
    API -->|Reads/writes game data<br/>via Azure SDK| TableDB
    API -->|Stores logs and files<br/>via Azure SDK| BlobStore
    
    %% Monitoring & Logging
    WebApp -->|Sends client telemetry<br/>Performance data| AppInsights
    API -->|Sends server telemetry<br/>Request/dependency tracking| AppInsights
    API -->|Writes structured logs<br/>Error and diagnostic data| LogAnalytics
    
    %% Styling
    classDef person fill:#08427b,color:#ffffff
    classDef webapp fill:#1168bd,color:#ffffff
    classDef api fill:#1168bd,color:#ffffff
    classDef database fill:#ff6b6b,color:#ffffff
    classDef external fill:#999999,color:#ffffff
    
    class Users person
    class WebApp webapp
    class API api
    class TableDB,BlobStore database
    class Browser,AppInsights,LogAnalytics external
```
