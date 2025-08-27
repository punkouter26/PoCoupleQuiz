# Convert Mermaid Diagrams to SVG
# This script requires mermaid-cli to be installed: npm install -g @mermaid-js/mermaid-cli

$diagramsPath = "c:\Users\punko\Downloads\PoCoupleQuiz\Diagrams"
$mdFiles = Get-ChildItem -Path $diagramsPath -Filter "*.md"

Write-Host "Converting Mermaid diagrams to SVG format..." -ForegroundColor Green

foreach ($file in $mdFiles) {
    $inputPath = $file.FullName
    $outputPath = $inputPath -replace "\.md$", ".svg"
    $fileName = $file.BaseName
    
    Write-Host "Converting $fileName..." -ForegroundColor Yellow
    
    try {
        # Extract mermaid content from markdown file
        $content = Get-Content $inputPath -Raw
        $mermaidContent = $content -replace '```mermaid\s*', '' -replace '\s*```', ''
        
        # Create temporary mermaid file
        $tempFile = Join-Path $diagramsPath "temp_$fileName.mmd"
        $mermaidContent | Out-File -FilePath $tempFile -Encoding UTF8
        
        # Convert to SVG using mermaid-cli
        $command = "mmdc -i `"$tempFile`" -o `"$outputPath`" -t dark --backgroundColor transparent"
        Invoke-Expression $command
        
        # Clean up temporary file
        Remove-Item $tempFile -Force
        
        Write-Host "✓ Created $fileName.svg" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Failed to convert $fileName`: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`nConversion complete! SVG files created in Diagrams folder." -ForegroundColor Green
Write-Host "Note: If mermaid-cli is not installed, run: npm install -g @mermaid-js/mermaid-cli" -ForegroundColor Cyan
