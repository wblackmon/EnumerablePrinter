<#
.SYNOPSIS
  Deploys EnumerablePrinter to NuGet.org.

.DESCRIPTION
  Builds, tests, packs, and pushes the specified version to NuGet.org using your API key.

.EXAMPLE
  $env:NUGET_API_KEY = "your-nuget-api-key"
  .\deploy.ps1 -Version 1.0.5

.PARAMETER Version
  The semantic version to deploy (e.g., 1.0.5). Must match the .csproj version.

.PARAMETER ProjectPath
  Path to the .csproj file. Default: src/EnumerablePrinter/EnumerablePrinter.csproj

.PARAMETER OutputDir
  Directory to output the .nupkg file. Default: nupkg

.PARAMETER DryRun
  If specified, skips the NuGet push step.
#>

param (
    [string]$Version = "1.0.0",
    [string]$ProjectPath = "src/EnumerablePrinter/EnumerablePrinter.csproj",
    [string]$OutputDir = "nupkg",
    [switch]$DryRun
)

# USAGE:
#   $env:NUGET_API_KEY = "your-nuget-api-key"
#   .\deploy.ps1 -Version 1.0.5
#   .\deploy.ps1 -Version 1.0.5 -DryRun

function Write-Log {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "$timestamp - $Message"
}

# Ensure NuGet API key is available
if (-not $env:NUGET_API_KEY) {
    throw "❌ NuGet API key not set. Run: `$env:NUGET_API_KEY = 'your-key'"
}

function Check-PackageExists {
    $url = "https://api.nuget.org/v3-flatcontainer/enumerableprinter/$Version/enumerableprinter.$Version.nupkg"
    try {
        $response = Invoke-WebRequest -Uri $url -Method Head -UseBasicParsing
        return $response.StatusCode -eq 200
    } catch {
        return $false
    }
}

function Build-And-Pack {
    Write-Log "🔧 Restoring project..."
    dotnet restore $ProjectPath

    Write-Log "🛠️ Building project..."
    dotnet build $ProjectPath -c Release

    Write-Log "🧪 Running tests..."
    dotnet test tests/EnumerablePrinter.Tests/EnumerablePrinter.Tests.csproj --no-build

    Write-Log "📦 Packing version $Version..."
    dotnet pack $ProjectPath -c Release -o $OutputDir /p:PackageVersion=$Version
}

function Push-ToNuGet {
    $packagePath = Get-ChildItem "$OutputDir\EnumerablePrinter.$Version.nupkg" -ErrorAction Stop
    Write-Log "📦 Found package: $($packagePath.Name)"

    if ($DryRun) {
        Write-Log "🧪 Dry run enabled. Skipping NuGet push."
        return
    }

    Write-Log "📤 Pushing to NuGet..."
    dotnet nuget push $packagePath.FullName `
        --api-key $env:NUGET_API_KEY `
        --source https://api.nuget.org/v3/index.json
}

# Main
Write-Log "🚀 Starting deploy for version $Version"

if (Check-PackageExists) {
    Write-Log "⚠️ Version $Version already exists on NuGet. Skipping push."
    exit 0
}

Build-And-Pack
Push-ToNuGet
Write-Log "✅ Deploy complete."