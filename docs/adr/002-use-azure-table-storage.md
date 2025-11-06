# ADR 002: Use Azure Table Storage for Data Persistence

## Status
Accepted

## Context
The application needs to persist:
- Player information
- Game sessions
- Question sets
- Game history and statistics

We evaluated several Azure data storage options:
- **Azure SQL Database**: Relational, full ACID compliance, higher cost
- **Azure Cosmos DB**: Global distribution, multi-model, higher cost
- **Azure Table Storage**: NoSQL key-value store, cost-effective, simple schema

## Decision
We will use **Azure Table Storage** as our primary data persistence layer.

## Rationale
1. **Cost-Effective**: Significantly cheaper than SQL or Cosmos DB for our scale
2. **Simple Data Model**: Our entities (Players, Games, Questions) fit well in a key-value model
3. **Scalability**: Automatically scales to handle high throughput
4. **Azure Integration**: First-class support in Azure SDK with Azure.Data.Tables
5. **Local Development**: Azurite provides excellent local emulation
6. **Query Flexibility**: Sufficient query capabilities for our use cases (PartitionKey + RowKey queries)

## Consequences

### Positive
- Very low operational cost (pay-per-use pricing)
- No database schema migrations needed
- Azurite enables offline development without Azure connectivity
- Automatic scaling and high availability
- Simple SDK with async/await support
- Easy backup and disaster recovery via Azure Storage

### Negative
- No complex joins or relationships (requires denormalization)
- Limited query capabilities (no full-text search, complex filters)
- No ACID transactions across partitions
- Partition key design is critical for performance
- Learning curve for NoSQL patterns (thinking in terms of PartitionKey/RowKey)

## Implementation Notes
- **Partition Strategy**: Use GameId as PartitionKey for game-related entities
- **RowKey Strategy**: Use composite keys (e.g., `Player_{PlayerId}`, `Question_{QuestionId}`)
- **Connection String**: Store in User Secrets (local) and Key Vault (production)
- **Azurite**: Use `UseDevelopmentStorage=true` for local development
- **Health Checks**: Implement custom health check to verify Table Storage connectivity

### Table Design
| Table Name | PartitionKey | RowKey | Purpose |
|------------|-------------|--------|---------|
| Games | GameId | `Game_{GameId}` | Game sessions |
| Players | GameId | `Player_{PlayerId}` | Player info per game |
| Questions | Category | `Question_{QuestionId}` | Question bank |
| GameHistory | UserId | `Game_{Timestamp}` | Historical games |

## Alternatives Considered

### Azure SQL Database
- **Pros**: Full relational capabilities, strong consistency, familiar SQL
- **Cons**: Higher cost (~$5/month minimum), requires schema migrations
- **Why not chosen**: Overkill for simple key-value data model

### Azure Cosmos DB
- **Pros**: Global distribution, multi-model, better query capabilities
- **Cons**: Significantly higher cost, more complex setup
- **Why not chosen**: Not needed for single-region, simple data model

## References
- [Azure Table Storage Documentation](https://learn.microsoft.com/en-us/azure/storage/tables/)
- [Azure.Data.Tables SDK](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/data.tables-readme)
- [Azurite Local Emulator](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite)
