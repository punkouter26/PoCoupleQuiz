# ADR 007: Use SignalR for Real-Time Multiplayer

## Status
Accepted

## Context
PoCoupleQuiz is a multiplayer game where players need to see real-time updates:
- When other players join or leave the game
- When the King player submits their answer
- When guessing players submit their answers
- When round results are calculated
- When the game ends

Currently, the application uses client-side state management with no synchronization between players. Each player operates independently, which works for single-device scenarios but limits true multiplayer functionality.

We need a real-time communication mechanism that:
- Works with Blazor WebAssembly
- Scales with Azure Container Apps
- Has low latency for game interactions
- Supports automatic reconnection
- Integrates well with .NET ecosystem

## Decision
We will use **ASP.NET Core SignalR** for real-time bidirectional communication between clients and server.

## Rationale

1. **Native .NET Integration**: SignalR is a first-party Microsoft library with excellent Blazor support, eliminating third-party dependencies
2. **Automatic Transport Negotiation**: SignalR automatically selects the best transport (WebSockets → Server-Sent Events → Long Polling)
3. **Group-Based Messaging**: Built-in support for grouping connections by game ID, perfect for our game room model
4. **Automatic Reconnection**: The client library handles reconnection automatically with configurable retry policies
5. **Scalability**: Azure SignalR Service can be added later for horizontal scaling without code changes
6. **Type Safety**: Strongly-typed hub methods with compile-time checking

## Alternatives Considered

### Alternative 1: Polling
- **Pros**: Simple to implement, works everywhere
- **Cons**: High latency (seconds), server load, battery drain on mobile
- **Why rejected**: Poor user experience for real-time game interactions

### Alternative 2: WebSocket Raw Implementation
- **Pros**: Lower overhead than SignalR
- **Cons**: No automatic reconnection, no fallback transports, more code to write
- **Why rejected**: SignalR provides these features out-of-the-box

### Alternative 3: Third-Party (Pusher, Ably, Socket.io)
- **Pros**: Feature-rich, managed service
- **Cons**: External dependency, additional cost, less .NET integration
- **Why rejected**: SignalR is native to .NET and can use Azure SignalR Service if needed

## Consequences

### Positive
- Real-time game state synchronization across all players
- Improved multiplayer experience with instant feedback
- Reduced server load compared to polling
- Foundation for future real-time features (chat, live reactions)
- Seamless integration with existing ASP.NET Core pipeline

### Negative
- Additional complexity in state management
- WebSocket connections consume server resources
- Need to handle connection lifecycle (connect, reconnect, disconnect)
- Potential for race conditions in rapid state changes

### Risks
- **Connection scalability**: Mitigated by Azure SignalR Service for production
- **State consistency**: Mitigated by server-authoritative game state
- **Mobile network reliability**: Mitigated by SignalR's automatic reconnection

## Implementation Notes

### Server Setup
```csharp
// Program.cs
builder.Services.AddSignalR();
app.MapHub<GameHub>("/hubs/game");
```

### Hub Methods
- `JoinGame(gameId, playerName)` - Join a game room
- `LeaveGame(gameId, playerName)` - Leave a game room
- `KingAnswered(gameId, roundIndex)` - Notify King answered
- `PlayerAnswered(gameId, playerName, roundIndex)` - Notify player answered
- `RoundCompleted(gameId, roundIndex, scores)` - Broadcast results
- `GameCompleted(gameId, finalScores)` - End game notification

### Client Events
- `OnPlayerJoined` - Another player joined
- `OnPlayerLeft` - Another player left
- `OnKingAnswered` - King submitted answer
- `OnPlayerAnswered` - Player submitted answer
- `OnRoundCompleted` - Round finished with scores
- `OnGameCompleted` - Game ended

### Future Scaling (Azure SignalR Service)
```csharp
builder.Services.AddSignalR()
    .AddAzureSignalR(connectionString);
```

## References
- [ASP.NET Core SignalR Documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/)
- [Blazor WebAssembly SignalR Client](https://learn.microsoft.com/en-us/aspnet/core/blazor/tutorials/signalr-blazor)
- [Azure SignalR Service](https://learn.microsoft.com/en-us/azure/azure-signalr/)
- [ADR-002: Azure Table Storage](002-use-azure-table-storage.md)

---
*Date: 2026-02-01*
*Author: PoCoupleQuiz Team*
