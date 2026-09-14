<#
.SYNOPSIS
  Runs checks, commits current changes, and pushes main.

.DESCRIPTION
  This script does not bump versions, create tags, or publish packages.
#>

param (
    [string]$Message = "Update EnumerablePrinter",
    [switch]$DryRun
)

$TestProjectPath = "tests/EnumerablePrinter.Tests/EnumerablePrinter.Tests.csproj"

function Write-Log {
    param([string]$Message)
    Write-Host "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - $Message"
}

function Invoke-Checks {
    Write-Log "🔍 Running build and test checks..."
    dotnet build $TestProjectPath --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "❌ Build failed." }

    dotnet test $TestProjectPath --no-build --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "❌ Tests failed." }

    Write-Log "✅ Checks passed."
}

Invoke-Checks

if ($DryRun) {
    Write-Log "🧪 Dry run enabled. Skipping stage, commit, pull, and push."
    exit 0
}

Write-Log "📦 Staging changes..."
git add .
if ($LASTEXITCODE -ne 0) { throw "❌ Staging failed." }

git diff --cached --quiet
if ($LASTEXITCODE -eq 0) {
    Write-Log "ℹ️ No changes to commit."
    exit 0
}

Write-Log "📝 Committing: '$Message'"
git commit -m $Message
if ($LASTEXITCODE -ne 0) { throw "❌ Commit failed." }

Write-Log "🔄 Rebasing from origin/main..."
git pull --rebase origin main
if ($LASTEXITCODE -ne 0) { throw "❌ Rebase failed." }

Write-Log "🚀 Pushing to origin/main..."
git push origin main
if ($LASTEXITCODE -ne 0) { throw "❌ Push failed." }

Write-Log "✅ Check-in complete."