<#
.SYNOPSIS
  Publishes the current EnumerablePrinter package to NuGet.org.

.DESCRIPTION
  Builds, tests, packs, and pushes the current package version to NuGet.
  This script assumes the project version has already been bumped and tagged
    via the release script. Local publishing uses an API key; GitHub Actions
    publishes through NuGet Trusted Publishing.

.EXAMPLE
  .\deploy.ps1
  .\deploy.ps1 -DryRun
#>

param (
    [string]$ProjectPath = "src/EnumerablePrinter/EnumerablePrinter.csproj",
    [string]$TestProjectPath = "tests/EnumerablePrinter.Tests/EnumerablePrinter.Tests.csproj",
    [string]$OutputDir = "nupkg",
    [switch]$DryRun
)

function Write-Log {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "$timestamp - $Message"
}

$secretFile = "nuget.secret.ps1"
if (Test-Path $secretFile) {
    Write-Log "🔐 Loading NuGet API key from $secretFile"
    $secrets = Invoke-Expression (Get-Content $secretFile -Raw)
    if ($secrets.NuGetApiKey) {
        $env:NUGET_API_KEY = $secrets.NuGetApiKey
    }
}

if (-not $env:NUGET_API_KEY) {
    throw "❌ NuGet API key not set. Create nuget.secret.ps1 with @{ NuGetApiKey = 'your-key' }"
}

function Test-PackageExists($version) {
    $url = "https://api.nuget.org/v3-flatcontainer/enumerableprinter/$version/enumerableprinter.$version.nupkg"
    try {
        $response = Invoke-WebRequest -Uri $url -Method Head -UseBasicParsing
        return $response.StatusCode -eq 200
    } catch {
        return $false
    }
}

function Build-And-Pack($version) {
    Write-Log "🔧 Restoring project..."
    dotnet restore $ProjectPath

    Write-Log "🛠️ Building project..."
    dotnet build $ProjectPath -c Release

    Write-Log "🧪 Running tests..."
    dotnet test $TestProjectPath --no-build

    Write-Log "📦 Packing version $version..."
    dotnet pack $ProjectPath -c Release -o $OutputDir /p:PackageVersion=$version
}

function Push-ToNuGet($version) {
    $packagePath = Get-ChildItem "$OutputDir\EnumerablePrinter.$version.nupkg" -ErrorAction Stop
    Write-Log "📦 Found package: $($packagePath.Name)"

    if ($DryRun) {
        Write-Log "🧪 Dry run enabled. Skipping NuGet push."
        return
    }

    Write-Log "📤 Pushing to NuGet with API key..."
    dotnet nuget push $packagePath.FullName `
        --api-key $env:NUGET_API_KEY `
        --source https://api.nuget.org/v3/index.json
}

Write-Log "🚀 Starting deploy..."

$version = (Get-Content "VERSION" -Raw).Trim()
if (-not $version) {
    throw "VERSION file not found or empty. Run the release script first."
}

if (Test-PackageExists $version) {
    Write-Log "⚠️ Version $version already exists on NuGet. Skipping push."
    exit 0
}

Build-And-Pack $version
Push-ToNuGet $version

Write-Log "✅ Deploy complete."
