Write-Host "=== EnumerablePrinter Analyzer Setup ==="

# Ensure we are in the repo root
Write-Host "Current directory: $(Get-Location)"

# ----------------------------------------
# Create analyzer source files
# ----------------------------------------
Write-Host "Creating analyzer source files..."

dotnet new class -n PrintMethodAnalyzer -o src/EnumerablePrinter.Analyzers
dotnet new class -n DiagnosticDescriptors -o src/EnumerablePrinter.Analyzers

# ----------------------------------------
# Create analyzer test file
# ----------------------------------------
Write-Host "Creating analyzer test file..."

dotnet new class -n PrintMethodAnalyzerTests -o tests/EnumerablePrinter.Analyzers.Tests

# ----------------------------------------
# Add Roslyn analyzer test packages
# ----------------------------------------
Write-Host "Adding Roslyn analyzer test packages..."

dotnet add tests/EnumerablePrinter.Analyzers.Tests package Microsoft.CodeAnalysis.CSharp
dotnet add tests/EnumerablePrinter.Analyzers.Tests package Microsoft.CodeAnalysis.CSharp.Workspaces
dotnet add tests/EnumerablePrinter.Analyzers.Tests package Microsoft.CodeAnalysis.NetAnalyzers
dotnet add tests/EnumerablePrinter.Analyzers.Tests package Microsoft.CodeAnalysis.CSharp.Analyzer.Testing.XUnit

# ----------------------------------------
# Reference analyzer project from test project
# ----------------------------------------
Write-Host "Adding project reference..."

dotnet add tests/EnumerablePrinter.Analyzers.Tests reference src/EnumerablePrinter.Analyzers/EnumerablePrinter.Analyzers.csproj

Write-Host "=== Analyzer setup complete ==="
