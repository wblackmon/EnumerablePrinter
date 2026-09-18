<#
    scripts/checkin.ps1 — Deterministic Check‑In Script

    LOCATION:
        This script resides in the /scripts folder.
        Run it from the repository root:
            .\scripts\checkin.ps1 <type> "<message>"

    PURPOSE:
        Provides controlled, semantic check-ins for the EnumerablePrinter repo.
        Supports three modes:
            - minor         → increments PATCH (1.2.3 → 1.2.4)
            - intermediate  → increments MINOR and resets PATCH (1.2.3 → 1.3.0)
            - nondeployable → no version bump; marks version as "-non"

    USAGE EXAMPLES:
        .\scripts\checkin.ps1 minor "Fixed formatting edge case"
        .\scripts\checkin.ps1 intermediate "Added new slicing semantics"
        .\scripts\checkin.ps1 nondeployable "Experimental refactor"

    BEHAVIOR:
        - Deterministic versioning
        - Full solution validation before changing git state
        - Workspace-local operations only
        - Silent, idempotent git commands
        - VERSION file is authoritative
        - No NuGet credentials or package publishing

    TRUSTED PUBLISHING:
        Check-in pushes source changes only. Run release.ps1 afterward to create
        and push a v*.*.* tag, which triggers GitHub Actions NuGet publishing.
#>

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("minor", "intermediate", "nondeployable")]
    [string]$Type,

    [string]$Message = ""
)

# ---------------------------
# Deterministic workspace setup
# ---------------------------
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
Set-Location $root

# ---------------------------
# Validation before mutation
# ---------------------------
Write-Host "Running solution build..."
dotnet build .\EnumerablePrinter.sln --nologo --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    throw "Solution build failed. No changes were committed or pushed."
}

Write-Host "Running solution tests..."
dotnet test .\EnumerablePrinter.sln --no-build --nologo --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    throw "Solution tests failed. No changes were committed or pushed."
}

# ---------------------------
# Version file
# ---------------------------
$versionFile = Join-Path $root "VERSION"
if (-not (Test-Path $versionFile)) {
    Write-Host "VERSION file not found. Aborting."
    exit 1
}

$current = Get-Content $versionFile | Select-Object -First 1
$parts = $current.Split(".")
if ($parts.Count -lt 3) {
    Write-Host "VERSION file must be MAJOR.MINOR.PATCH"
    exit 1
}

$major = [int]$parts[0]
$minor = [int]$parts[1]
$patch = [int]$parts[2]

# ---------------------------
# Version increment rules
# ---------------------------
switch ($Type) {
    "minor" {
        $patch += 1
    }
    "intermediate" {
        $minor += 1
        $patch = 0
    }
    "nondeployable" {
        $non = "$major.$minor.$patch-non"
        Set-Content -Path $versionFile -Value $non
        git add $versionFile
        git commit -m "non-deployable: $Message"
        git push
        Write-Host "Non-deployable check-in complete."
        exit 0
    }
}

# ---------------------------
# Write new version
# ---------------------------
$newVersion = "$major.$minor.$patch"
Set-Content -Path $versionFile -Value $newVersion

# ---------------------------
# Git operations
# ---------------------------
git add .
$commitMessage = "$Type check-in: $newVersion"
if ($Message -ne "") {
    $commitMessage += " — $Message"
}

git commit -m $commitMessage
git push

Write-Host "Check-in complete: $newVersion ($Type)"
Write-Host "Trusted publishing is not triggered by a branch push. Run .\scripts\release.ps1 to create and push the release tag."
