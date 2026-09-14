<#
.SYNOPSIS
  Creates a release version bump for EnumerablePrinter.

.DESCRIPTION
  Updates the VERSION file and project version, creates a git tag,
  and pushes the tag to the remote repository. This script intentionally
  does not package or publish to NuGet.
#>

param (
    [string]$ProjectPath = "src/EnumerablePrinter/EnumerablePrinter.csproj",
    [switch]$DryRun
)

function Write-Log {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "$timestamp - $Message"
}

function Get-Version {
    if (-not (Test-Path "VERSION")) {
        throw "VERSION file not found."
    }
    return (Get-Content "VERSION" -Raw).Trim()
}

function Set-Version($version) {
    Set-Content -Path "VERSION" -Value $version -NoNewline
}

function Step-Version {
    $version = Get-Version
    $segments = $version.Split('.')

    [int]$major = $segments[0]
    [int]$minor = $segments[1]
    [int]$patch = $segments[2]

    $patch++

    if ($patch -gt 9) {
        $patch = 0
        $minor++
    }

    if ($minor -gt 9) {
        $minor = 0
        $major++
    }

    $newVersion = "$major.$minor.$patch"
    return $newVersion
}

function Update-CsprojVersion($version) {
    Write-Log "📝 Updating .csproj version to $version"
    (Get-Content $ProjectPath) `
        -replace '<Version>.*</Version>', "<Version>$version</Version>" |
        Set-Content $ProjectPath
}

Write-Log "🚀 Starting release bump..."
$newVersion = Step-Version
$tag = "v$newVersion"

Write-Log "🔢 New version: $newVersion"
Write-Log "🏷️ Tag to be created: $tag"

if ($DryRun) {
    Write-Log "🧪 Dry run enabled. Skipping file updates, commit, tag, and push."
    Write-Log "✅ Release bump validation complete."
    exit 0
}

Set-Version $newVersion
Update-CsprojVersion $newVersion

Write-Log "📦 Staging version bump..."
git add VERSION
if (Test-Path $ProjectPath) {
    git add $ProjectPath
}

Write-Log "📝 Committing version bump..."
git commit -m "Release $newVersion"

Write-Log "🏷️ Tagging release..."
git tag $tag

git push origin main
git push origin $tag

Write-Log "✅ Release bump complete."
