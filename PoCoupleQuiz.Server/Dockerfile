# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY PoCoupleQuiz.sln ./
COPY PoCoupleQuiz.Core/*.csproj PoCoupleQuiz.Core/
COPY PoCoupleQuiz.Client/*.csproj PoCoupleQuiz.Client/
COPY PoCoupleQuiz.Server/*.csproj PoCoupleQuiz.Server/
COPY PoCoupleQuiz.ServiceDefaults/*.csproj PoCoupleQuiz.ServiceDefaults/

# Restore dependencies
RUN dotnet restore PoCoupleQuiz.Server/PoCoupleQuiz.Server.csproj

# Copy everything else
COPY PoCoupleQuiz.Core/ PoCoupleQuiz.Core/
COPY PoCoupleQuiz.Client/ PoCoupleQuiz.Client/
COPY PoCoupleQuiz.Server/ PoCoupleQuiz.Server/
COPY PoCoupleQuiz.ServiceDefaults/ PoCoupleQuiz.ServiceDefaults/

# Build and publish
WORKDIR /src/PoCoupleQuiz.Server
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Run the app
ENTRYPOINT ["dotnet", "PoCoupleQuiz.Server.dll"]
