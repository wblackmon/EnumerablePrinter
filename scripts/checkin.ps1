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
        - Workspace-local operations only
        - Silent, idempotent git commands
        - VERSION file is authoritative
        - No global environment pollution
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
