<#
.SYNOPSIS
  Validates and packages the current EnumerablePrinter release.

.DESCRIPTION
  Confirms the VERSION file matches the project version, runs the full
  Release test suite, and creates the EnumerablePrinter NuGet package.

  This script does not publish to NuGet.org.

  NuGet publication is handled by GitHub Actions using Trusted Publishing.

.EXAMPLE
  .\scripts\deploy.ps1

.EXAMPLE
  .\scripts\deploy.ps1 -DryRun
#>

param (
    [string]$ProjectPath =
        "src/EnumerablePrinter/EnumerablePrinter.csproj",

    [string]$SolutionPath =
        "EnumerablePrinter.sln",

    [string]$OutputDir =
        "nupkg",

    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

function Write-Log {
    param([string]$Message)

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "$timestamp - $Message"
}

function Assert-LastCommand {
    param([string]$Message)

    if ($LASTEXITCODE -ne 0) {
        throw $Message
    }
}

Write-Log "Starting deployment validation..."

# ------------------------------------------------------------
# Validate required files
# ------------------------------------------------------------

if (-not (Test-Path "VERSION")) {
    throw "VERSION file not found."
}

if (-not (Test-Path $ProjectPath)) {
    throw "Project file not found: $ProjectPath"
}

if (-not (Test-Path $SolutionPath)) {
    throw "Solution file not found: $SolutionPath"
}

# ------------------------------------------------------------
# Read VERSION
# ------------------------------------------------------------

$version = (Get-Content "VERSION" -Raw).Trim()

if (-not $version) {
    throw "VERSION file is empty."
}

if ($version -notmatch '^\d+\.\d+\.\d+$') {
    throw "Invalid VERSION value: $version"
}

Write-Log "VERSION: $version"

# ------------------------------------------------------------
# Validate project version
# ------------------------------------------------------------

$projectContent =
    Get-Content $ProjectPath -Raw

$versionMatch =
    [regex]::Match(
        $projectContent,
        '<Version>([^<]+)</Version>')

if (-not $versionMatch.Success) {
    throw "Could not find <Version> in $ProjectPath."
}

$projectVersion =
    $versionMatch.Groups[1].Value.Trim()

Write-Log "Project version: $projectVersion"

if ($projectVersion -ne $version) {
    throw "VERSION is '$version' but project version is '$projectVersion'."
}

Write-Log "Version files match."

# ------------------------------------------------------------
# Basic Git validation
# ------------------------------------------------------------

Write-Log "Checking Git working tree..."

git status --porcelain

Assert-LastCommand "Unable to inspect Git working tree."

$gitStatus =
    git status --porcelain

Assert-LastCommand "Unable to inspect Git working tree."

if ($gitStatus) {
    throw "Working tree is not clean. Commit or stash changes first."
}

$currentBranch =
    git branch --show-current

Assert-LastCommand "Unable to determine current Git branch."

if ($currentBranch.Trim() -ne "main") {
    throw "Deployment validation must run from the main branch."
}

Write-Log "Git working tree is clean."
Write-Log "Current branch is main."

# ------------------------------------------------------------
# Restore
# ------------------------------------------------------------

Write-Log "Restoring solution..."

dotnet restore $SolutionPath --nologo

Assert-LastCommand "Restore failed."

# ------------------------------------------------------------
# Build and test
# ------------------------------------------------------------

Write-Log "Running Release build and tests..."

dotnet test `
    $SolutionPath `
    --configuration Release `
    --no-restore `
    --nologo

Assert-LastCommand "Release build or tests failed."

Write-Log "Release build and tests passed."

# ------------------------------------------------------------
# Prepare package directory
# ------------------------------------------------------------

if (Test-Path $OutputDir) {
    Write-Log "Removing previous package output..."

    Remove-Item `
        $OutputDir `
        -Recurse `
        -Force
}

New-Item `
    $OutputDir `
    -ItemType Directory `
    -Force |
    Out-Null

# ------------------------------------------------------------
# Pack
# ------------------------------------------------------------

Write-Log "Packing EnumerablePrinter $version..."

dotnet pack `
    $ProjectPath `
    --configuration Release `
    --no-restore `
    --no-build `
    --output $OutputDir `
    /p:PackageVersion=$version

Assert-LastCommand "Package creation failed."

# ------------------------------------------------------------
# Verify package
# ------------------------------------------------------------

$packagePath =
    Join-Path `
        $OutputDir `
        "EnumerablePrinter.$version.nupkg"

if (-not (Test-Path $packagePath)) {
    throw "Expected package was not created: $packagePath"
}

$package =
    Get-Item $packagePath

if ($package.Length -le 0) {
    throw "Package is empty: $packagePath"
}

Write-Log "Package created successfully."
Write-Log "Package: $($package.FullName)"
Write-Log "Size: $($package.Length) bytes"

# ------------------------------------------------------------
# Complete
# ------------------------------------------------------------

if ($DryRun) {
    Write-Log "Dry run complete."
}

Write-Log "Deployment validation passed."
Write-Log "No package was published locally."
Write-Log "NuGet publication is handled by GitHub Actions Trusted Publishing."