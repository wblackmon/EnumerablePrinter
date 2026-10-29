<#
.SYNOPSIS
  Validates and packages the current EnumerablePrinter release.

.DESCRIPTION
  Validates the released version and Git state, builds and tests the
  solution in Release configuration, creates the NuGet package, and
  verifies the package contents.

  This script does not publish directly to NuGet.org.

  Package publication is handled by GitHub Actions using NuGet
  Trusted Publishing after the release tag is pushed.

.PARAMETER ProjectPath
  Path to the project that produces the NuGet package.

.PARAMETER SolutionPath
  Path to the solution used for Release build and test validation.

.PARAMETER OutputDir
  Directory in which the NuGet package is created.

.PARAMETER PackageId
  NuGet package identifier.

.PARAMETER Branch
  Branch from which deployment validation is permitted.

.PARAMETER Remote
  Git remote used to validate the release commit and tag.

.PARAMETER DryRun
  Retained for workflow compatibility. Since this script never publishes
  directly to NuGet, both normal and dry-run execution are non-publishing.

.EXAMPLE
  .\scripts\deploy.ps1 -DryRun

.EXAMPLE
  .\scripts\deploy.ps1
#>

[CmdletBinding()]
param (
    [string]$ProjectPath =
        "src/EnumerablePrinter/EnumerablePrinter.csproj",

    [string]$SolutionPath =
        "EnumerablePrinter.sln",

    [string]$OutputDir =
        "nupkg",

    [string]$PackageId =
        "EnumerablePrinter",

    [string]$Branch =
        "main",

    [string]$Remote =
        "origin",

    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Log {
    param(
        [Parameter(Mandatory)]
        [string]$Message
    )

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "$timestamp - $Message"
}

function Assert-CommandExists {
    param(
        [Parameter(Mandatory)]
        [string]$CommandName
    )

    $command =
        Get-Command $CommandName -ErrorAction SilentlyContinue

    if (-not $command) {
        throw "Required command '$CommandName' was not found."
    }
}

function Invoke-NativeCommand {
    param(
        [Parameter(Mandatory)]
        [scriptblock]$Command,

        [Parameter(Mandatory)]
        [string]$FailureMessage
    )

    & $Command

    if ($LASTEXITCODE -ne 0) {
        throw $FailureMessage
    }
}

function Get-NativeCommandOutput {
    param(
        [Parameter(Mandatory)]
        [scriptblock]$Command,

        [Parameter(Mandatory)]
        [string]$FailureMessage
    )

    $output = & $Command

    if ($LASTEXITCODE -ne 0) {
        throw $FailureMessage
    }

    return $output
}

function Get-RepositoryRoot {
    $root = Get-NativeCommandOutput `
        -Command {
            git rev-parse --show-toplevel 2>$null
        } `
        -FailureMessage "The current directory is not inside a Git repository."

    if (-not $root) {
        throw "Unable to determine the Git repository root."
    }

    return ($root | Select-Object -First 1).Trim()
}

function Get-GitDirectory {
    $gitDirectory = Get-NativeCommandOutput `
        -Command {
            git rev-parse --git-dir
        } `
        -FailureMessage "Unable to determine the Git metadata directory."

    $gitDirectory =
        ($gitDirectory | Select-Object -First 1).Trim()

    if (-not [System.IO.Path]::IsPathRooted($gitDirectory)) {
        $gitDirectory =
            Join-Path $script:RepositoryRoot $gitDirectory
    }

    return [System.IO.Path]::GetFullPath($gitDirectory)
}

function Assert-NoGitOperationInProgress {
    $gitDirectory = Get-GitDirectory

    $operationMarkers = @(
        @{
            Path = "MERGE_HEAD"
            Name = "merge"
        },
        @{
            Path = "rebase-merge"
            Name = "rebase"
        },
        @{
            Path = "rebase-apply"
            Name = "rebase"
        },
        @{
            Path = "CHERRY_PICK_HEAD"
            Name = "cherry-pick"
        },
        @{
            Path = "REVERT_HEAD"
            Name = "revert"
        },
        @{
            Path = "BISECT_LOG"
            Name = "bisect"
        }
    )

    foreach ($marker in $operationMarkers) {
        $markerPath =
            Join-Path $gitDirectory $marker.Path

        if (Test-Path $markerPath) {
            throw "A Git $($marker.Name) operation is in progress. Complete or abort it before deployment."
        }
    }

    Write-Log "Git has no merge, rebase, cherry-pick, revert, or bisect operation in progress."
}

function Get-Version {
    $versionFile =
        Join-Path $script:RepositoryRoot "VERSION"

    if (-not (Test-Path $versionFile)) {
        throw "VERSION file was not found."
    }

    $version =
        (Get-Content $versionFile -Raw).Trim()

    if ([string]::IsNullOrWhiteSpace($version)) {
        throw "VERSION file is empty."
    }

    if ($version -notmatch '^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$') {
        throw "VERSION '$version' is not a valid major.minor.patch version."
    }

    return $version
}

function Get-ProjectVersion {
    $fullProjectPath =
        Join-Path $script:RepositoryRoot $ProjectPath

    if (-not (Test-Path $fullProjectPath)) {
        throw "Project file was not found: $ProjectPath"
    }

    $content =
        [System.IO.File]::ReadAllText($fullProjectPath)

    $matches =
        [System.Text.RegularExpressions.Regex]::Matches(
            $content,
            '<Version>\s*([^<]+?)\s*</Version>')

    if ($matches.Count -ne 1) {
        throw "Expected exactly one <Version> element in '$ProjectPath', but found $($matches.Count)."
    }

    $version =
        $matches[0].Groups[1].Value.Trim()

    if ($version -notmatch '^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$') {
        throw "Project version '$version' is not a valid major.minor.patch version."
    }

    return $version
}

function Assert-VersionState {
    $version = Get-Version
    $projectVersion = Get-ProjectVersion

    if ($version -ne $projectVersion) {
        throw "VERSION contains '$version', but '$ProjectPath' contains '$projectVersion'."
    }

    $script:Version = $version
    $script:ReleaseTag = "v$version"

    Write-Log "VERSION and project version both contain '$version'."
    Write-Log "Expected release tag: '$script:ReleaseTag'."
}

function Assert-RemoteExists {
    $remotes = @(
        Get-NativeCommandOutput `
            -Command {
                git remote
            } `
            -FailureMessage "Unable to inspect Git remotes."
    )

    if ($Remote -notin $remotes) {
        throw "Git remote '$Remote' does not exist."
    }

    $remoteUrl = Get-NativeCommandOutput `
        -Command {
            git remote get-url $Remote
        } `
        -FailureMessage "Unable to read the URL for remote '$Remote'."

    $remoteUrl =
        ($remoteUrl | Select-Object -First 1).Trim()

    Write-Log "Remote '$Remote': $remoteUrl"
}

function Assert-GitState {
    Write-Log "Validating Git release state..."

    Assert-NoGitOperationInProgress
    Assert-RemoteExists

    $currentBranch = Get-NativeCommandOutput `
        -Command {
            git symbolic-ref --quiet --short HEAD
        } `
        -FailureMessage "Detached HEAD detected."

    $currentBranch =
        ($currentBranch | Select-Object -First 1).Trim()

    if ($currentBranch -ne $Branch) {
        throw "Deployment must run from '$Branch'. Current branch is '$currentBranch'."
    }

    Write-Log "Branch '$Branch' is checked out."

    $status = Get-NativeCommandOutput `
        -Command {
            git status --porcelain
        } `
        -FailureMessage "Unable to inspect the Git working tree."

    if ($status) {
        throw "The working tree is not clean. Commit or stash all changes before deployment."
    }

    Write-Log "Working tree is clean."

    Write-Log "Fetching '$Remote'..."

    Invoke-NativeCommand `
        -Command {
            git fetch --prune --tags $Remote
        } `
        -FailureMessage "Unable to fetch from '$Remote'."

    $remoteBranchReference =
        "refs/remotes/$Remote/$Branch"

    Invoke-NativeCommand `
        -Command {
            git show-ref --verify --quiet $remoteBranchReference
        } `
        -FailureMessage "Remote branch '$Remote/$Branch' does not exist."

    $localCommit = Get-NativeCommandOutput `
        -Command {
            git rev-parse HEAD
        } `
        -FailureMessage "Unable to determine the local commit."

    $remoteCommit = Get-NativeCommandOutput `
        -Command {
            git rev-parse "$Remote/$Branch"
        } `
        -FailureMessage "Unable to determine '$Remote/$Branch'."

    $localCommit =
        ($localCommit | Select-Object -First 1).Trim()

    $remoteCommit =
        ($remoteCommit | Select-Object -First 1).Trim()

    if ($localCommit -ne $remoteCommit) {
        throw "Local HEAD does not match '$Remote/$Branch'."
    }

    Write-Log "Local HEAD matches '$Remote/$Branch'."

    $tagCommit = Get-NativeCommandOutput `
        -Command {
            git rev-list -n 1 $script:ReleaseTag
        } `
        -FailureMessage "Release tag '$script:ReleaseTag' does not exist locally."

    $tagCommit =
        ($tagCommit | Select-Object -First 1).Trim()

    if ($tagCommit -ne $localCommit) {
        throw "Release tag '$script:ReleaseTag' does not point to the current commit."
    }

    Write-Log "Local tag '$script:ReleaseTag' points to the current commit."

    $remoteTagOutput = Get-NativeCommandOutput `
        -Command {
            git ls-remote `
                --tags `
                $Remote `
                "refs/tags/$script:ReleaseTag" `
                "refs/tags/$script:ReleaseTag^{}"
        } `
        -FailureMessage "Unable to inspect release tag '$script:ReleaseTag' on '$Remote'."

    if (-not $remoteTagOutput) {
        throw "Release tag '$script:ReleaseTag' has not been pushed to '$Remote'."
    }

    Write-Log "Release tag '$script:ReleaseTag' exists on '$Remote'."
}

function Clear-PackageOutput {
    $fullOutputDir =
        Join-Path $script:RepositoryRoot $OutputDir

    if (Test-Path $fullOutputDir) {
        Write-Log "Removing previous package output..."

        Remove-Item `
            -Path $fullOutputDir `
            -Recurse `
            -Force
    }

    New-Item `
        -Path $fullOutputDir `
        -ItemType Directory `
        -Force |
        Out-Null

    return $fullOutputDir
}

function Build-Test-And-Pack {
    $fullProjectPath =
        Join-Path $script:RepositoryRoot $ProjectPath

    $fullSolutionPath =
        Join-Path $script:RepositoryRoot $SolutionPath

    if (-not (Test-Path $fullProjectPath)) {
        throw "Project file was not found: $ProjectPath"
    }

    if (-not (Test-Path $fullSolutionPath)) {
        throw "Solution file was not found: $SolutionPath"
    }

    $fullOutputDir =
        Clear-PackageOutput

    Write-Log "Restoring solution..."

    Invoke-NativeCommand `
        -Command {
            dotnet restore $fullSolutionPath --nologo
        } `
        -FailureMessage "Solution restore failed."

    Write-Log "Building and testing Release configuration..."

    Invoke-NativeCommand `
        -Command {
            dotnet test $fullSolutionPath `
                --configuration Release `
                --no-restore `
                --nologo
        } `
        -FailureMessage "Release build or tests failed."

    Write-Log "Packing '$PackageId $script:Version'..."

    Invoke-NativeCommand `
        -Command {
            dotnet pack $fullProjectPath `
                --configuration Release `
                --no-restore `
                --no-build `
                --output $fullOutputDir `
                /p:PackageVersion=$script:Version
        } `
        -FailureMessage "Package creation failed."

    $expectedPackagePath =
        Join-Path `
            $fullOutputDir `
            "$PackageId.$script:Version.nupkg"

    if (-not (Test-Path $expectedPackagePath)) {
        throw "Expected package was not created: $expectedPackagePath"
    }

    $package =
        Get-Item $expectedPackagePath

    if ($package.Length -le 0) {
        throw "Package '$($package.FullName)' is empty."
    }

    $script:PackagePath =
        $package.FullName

    Write-Log "Package created: $($package.Name)"
    Write-Log "Package size: $($package.Length) bytes"
}

function Test-PackageArchive {
    if ([string]::IsNullOrWhiteSpace($script:PackagePath)) {
        throw "No package is available for inspection."
    }

    Write-Log "Inspecting NuGet package archive..."

    Add-Type -AssemblyName System.IO.Compression.FileSystem

    $archive =
        [System.IO.Compression.ZipFile]::OpenRead(
            $script:PackagePath)

    try {
        $entryNames = @(
            $            ForEach-Object {
                $_.FullName
            }
        )

        $requiredEntries = @(
            "$PackageId.nuspec",
            "lib/netstandard2.0/$PackageId.dll",
            "lib/net8.0/$PackageId.dll"
        )

        foreach ($requiredEntry in $requiredEntries) {
            if ($requiredEntry -notin $entryNames) {
                throw "Package is missing required entry '$requiredEntry'."
            }
        }

        $nuspecEntry =
            $archive.Entries |
            Where-Object {
                $_.FullName -eq "$PackageId.nuspec"
            } |
            Select-Object -First 1

        if (-not $nuspecEntry) {
            throw "Package does not contain '$PackageId.nuspec'."
        }

        $reader =
            New-Object System.IO.StreamReader(
                $nuspecEntry.Open())

        try {
            $nuspecContent =
                $reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }

        [xml]$nuspec =
            $nuspecContent

        $nuspecId =
            $nuspec.package.metadata.id

        $nuspecVersion =
            $nuspec.package.metadata.version

        if ($nuspecId -ne $PackageId) {
            throw "Package ID is '$nuspecId'; expected '$PackageId'."
        }

        if ($nuspecVersion -ne $script:Version) {
            throw "Package version is '$nuspecVersion'; expected '$script:Version'."
        }

        Write-Log "Package ID is '$nuspecId'."
        Write-Log "Package version is '$nuspecVersion'."
        Write-Log "Package contains the expected target-framework assemblies."
    }
    finally {
        $archive.Dispose()
    }
}

Assert-CommandExists -CommandName "git"
Assert-CommandExists -CommandName "dotnet"

$originalLocation = Get-Location
$script:RepositoryRoot = $null
$script:Version = $null
$script:ReleaseTag = $null
$script:PackagePath = $null

try {
    $script:RepositoryRoot =
        Get-RepositoryRoot

    Set-Location $script:RepositoryRoot

    Write-Log "Starting deployment validation..."
    Write-Log "Repository: $script:RepositoryRoot"

    Assert-VersionState
    Assert-GitState

    Build-Test-And-Pack
    Test-PackageArchive

    if ($DryRun) {
        Write-Log "Dry run enabled."
    }

    Write-Log "Release, Git, build, test, and package checks passed."
    Write-Log "Package ready: $script:PackagePath"
    Write-Log "No package was published locally."
    Write-Log "NuGet publication is handled by GitHub Actions using Trusted Publishing."
    Write-Log "Deployment validation complete."
}
catch {
    Write-Log "ERROR: $($_.Exception.Message)"
    exit 1
}
finally {
    Set-Location $originalLocation
}