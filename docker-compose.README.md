# Docker Compose for PoCoupleQuiz Local Development

This docker-compose configuration provides local development dependencies for PoCoupleQuiz.

## Services

### Azurite
Azure Storage emulator for local development:
- **Blob Service**: Port 10000
- **Queue Service**: Port 10001  
- **Table Service**: Port 10002

Data is persisted in the `azurite-poquiz-data` volume.

## Usage

### Start all services:
```powershell
docker-compose up -d
```

### View logs:
```powershell
docker-compose logs -f
```

### Stop all services:
```powershell
docker-compose down
```

### Stop and remove all data:
```powershell
docker-compose down -v
```

### Check service health:
```powershell
docker-compose ps
```

## Connection Strings

When services are running, use these connection strings:

**Azurite Table Storage:**
```
DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;
```

## Troubleshooting

**Port conflicts:**
If ports 10000-10002 are already in use:
```powershell
# Find what's using the port
netstat -ano | findstr :10002

# Stop the conflicting service or change ports in docker-compose.yml
```

**Permission issues:**
Run PowerShell as Administrator if you encounter permission errors.

**Data persistence:**
Azurite data is stored in a named Docker volume. To reset:
```powershell
docker-compose down -v
docker-compose up -d
```
