Write-Host "Generating coverage report..." -ForegroundColor Cyan

# Clean previous coverage data
if (Test-Path "TestResults") { Remove-Item -Path "TestResults" -Recurse -Force }
if (Test-Path "docs/coverage") { Remove-Item -Path "docs/coverage" -Recurse -Force }

# Build
dotnet build PoCoupleQuiz.sln --configuration Release --verbosity minimal

# Run tests with coverage
dotnet test PoCoupleQuiz.sln --collect:"XPlat Code Coverage" --results-directory:"./TestResults" --configuration Release --no-build --verbosity minimal

# Install ReportGenerator if needed
if (-not (dotnet tool list --global | Select-String "reportgenerator")) {
    dotnet tool install --global dotnet-reportgenerator-globaltool
}

# Find coverage file
$coverageFile = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse | Select-Object -First 1
if (-not $coverageFile) {
    $coverageFile = Get-ChildItem -Path "TestResults" -Filter "coverage.opencover.xml" -Recurse | Select-Object -First 1
}

if ($coverageFile) {
    Write-Host "Coverage file: $($coverageFile.FullName)" -ForegroundColor Gray
    
    # Generate HTML report
    New-Item -ItemType Directory -Path "docs/coverage" -Force | Out-Null
    reportgenerator "-reports:$($coverageFile.FullName)" "-targetdir:docs/coverage/html" "-reporttypes:Html;Badges" "-verbosity:Warning"
    
    Write-Host "Coverage report generated: docs/coverage/html/index.html" -ForegroundColor Green
} else {
    Write-Host "ERROR: Coverage file not found!" -ForegroundColor Red
    exit 1
}
