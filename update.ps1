<#
.SYNOPSIS
  Updates EnumerablePrinter with gated check-in logic.

.DESCRIPTION
  Runs build and test checks, commits changes, pushes to main, optionally tags and deploys.

.EXAMPLE
  .\update.ps1 -Message "Fix README.md" -Tag "v1.0.5" -Deploy

.PARAMETER Message
  Commit message to use.

.PARAMETER Tag
  Optional Git tag to create and push.

.PARAMETER Deploy
  If specified, runs deploy.ps1 with the given tag version.
#>

param (
    [string]$Message = "Update EnumerablePrinter",
    [string]$Tag = "",
    [switch]$Deploy
)

function Write-Log($msg) {
    $ts = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "$ts - $msg"
}

function Run-Checks {
    Write-Log "🔍 Running build and test checks..."
    dotnet build --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "❌ Build failed." }

    dotnet test --no-build --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "❌ Tests failed." }

    Write-Log "✅ Checks passed."
}

Run-Checks

Write-Log "📦 Staging changes..."
git add .

Write-Log "📝 Committing: '$Message'"
git commit -m $Message

Write-Log "🚀 Pushing to origin/main..."
git push origin main

if ($Tag) {
    Write-Log "🏷️ Tagging release: $Tag"
    git tag $Tag
    git push origin $Tag
}

if ($Deploy) {
    Write-Log "📤 Running deploy script..."
    .\deploy.ps1 -Version $Tag
}

Write-Log "✅ Update complete."
