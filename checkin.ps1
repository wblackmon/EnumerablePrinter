<#
.SYNOPSIS
  Gated check‑in script for EnumerablePrinter.

.DESCRIPTION
  Runs build and test checks, commits changes, rebases, and pushes to main.
  Does NOT bump version, tag, or deploy. Version changes occur ONLY during deploy.

.EXAMPLE
  .\checkin.ps1 -Message "Fix README.md"

.PARAMETER Message
  Commit message to use.
#>

param (
    [string]$Message = "Update EnumerablePrinter"
)

function Write-Log($msg) {
    $ts = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "$ts - $msg"
}

function Invoke-Checks {
    Write-Log "🔍 Running build and test checks..."
    dotnet build --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "❌ Build failed." }

    dotnet test --no-build --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "❌ Tests failed." }

    Write-Log "✅ Checks passed."
}

# Run checks
Invoke-Checks

Write-Log "📦 Staging changes..."
git add .

Write-Log "📝 Committing: '$Message'"
git commit -m "$Message"

# Handle rebase if needed
if (Test-Path ".git\rebase-merge") {
    Write-Log "🔄 Rebase in progress. Attempting to continue..."
    git rebase --continue
}

Write-Log "🔄 Rebasing from origin/main..."
git pull --rebase origin main

Write-Log "🚀 Pushing to origin/main..."
git push origin main

Write-Log "✅ Check‑in complete (no version bump, no tag, no deploy)."
