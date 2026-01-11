<#
.SYNOPSIS
  Deploys EnumerablePrinter to NuGet.org.

.DESCRIPTION
  Bumps the version (patch with rollover), updates VERSION and .csproj,
  commits the bump, tags the release, builds, tests, packs, and pushes
  the package to NuGet.org. Loads NuGet API key from nuget.secret.ps1
  if available.

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

# Load NuGet API key from nuget.secret.ps1 if present
$secretFile = "nuget.secret.ps1"
if (Test-Path $secretFile) {
    Write-Log "🔐 Loading NuGet API key from $secretFile"
    $secrets = Invoke-Expression (Get-Content $secretFile -Raw)
    if ($secrets.NuGetApiKey) {
        $env:NUGET_API_KEY = $secrets.NuGetApiKey
    }
}

# Ensure NuGet API key is available
if (-not $env:NUGET_API_KEY) {
    throw "❌ NuGet API key not set. Create nuget.secret.ps1 with @{ NuGetApiKey = 'your-key' }"
}

function Get-Version {
    if (-not (Test-Path VERSION)) {
        throw "VERSION file not found."
    }
    return (Get-Content VERSION -Raw).Trim()
}

function Set-Version($version) {
    Set-Content -Path VERSION -Value $version -NoNewline
}

function Step-Version {
    $version = Get-Version
    $segments = $version.Split('.')

    [int]$major = $segments[0]
    [int]$minor = $segments[1]
    [int]$patch = $segments[2]

    # Increment patch
    $patch++

    # Handle rollover
    if ($patch -gt 9) {
        $patch = 0
        $minor++
    }

    if ($minor -gt 9) {
        $minor = 0
        $major++
    }

    $newVersion = "$major.$minor.$patch"
    Set-Version $newVersion
    return $newVersion
}

function Update-CsprojVersion($version) {
    Write-Log "📝 Updating .csproj version to $version"
    (Get-Content $ProjectPath) `
        -replace '<Version>.*</Version>', "<Version>$version</Version>" |
        Set-Content $ProjectPath
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

    Write-Log "📤 Pushing to NuGet..."
    dotnet nuget push $packagePath.FullName `
        --api-key $env:NUGET_API_KEY `
        --source https://api.nuget.org/v3/index.json
}

# MAIN
Write-Log "🚀 Starting deploy..."

# 1. Bump version
$newVersion = Step-Version
$Tag = "v$newVersion"

Write-Log "🔢 New version: $newVersion"
Write-Log "🏷️ Tag to be created: $Tag"

# 2. Update .csproj
Update-CsprojVersion $newVersion

# 3. Commit version bump
Write-Log "📦 Staging version bump..."
git add VERSION
git add $ProjectPath

Write-Log "📝 Committing version bump..."
git commit -m "Release $newVersion"

# 4. Tag release
Write-Log "🏷️ Tagging release..."
git tag $Tag
git push origin main
git push origin $Tag

# 5. Skip if version already exists
if (Test-PackageExists $newVersion) {
    Write-Log "⚠️ Version $newVersion already exists on NuGet. Skipping push."
    exit 0
}

# 6. Build, test, pack, push
Build-And-Pack $newVersion
Push-ToNuGet $newVersion

Write-Log "✅ Deploy complete."
