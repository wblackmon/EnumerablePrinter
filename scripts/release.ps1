<#
.SYNOPSIS
  Creates a semantic-version release for EnumerablePrinter.

.DESCRIPTION
  Validates the Git repository, increments the selected semantic-version
  component, updates the VERSION file and project version, runs release
  checks, commits the version bump, creates an annotated Git tag, and pushes
  the release commit and tag.

  This script intentionally does not package or publish to NuGet.

.PARAMETER Bump
  Specifies which semantic-version component to increment:

    Patch: 2.1.3 -> 2.1.4
    Minor: 2.1.3 -> 2.2.0
    Major: 2.1.3 -> 3.0.0

.PARAMETER ProjectPath
  The path to the core EnumerablePrinter project, relative to the repository
  root.

.PARAMETER SolutionPath
  The solution used for release build and test validation.

.PARAMETER Branch
  The branch that must be checked out and released.

.PARAMETER Remote
  The Git remote used for synchronization and pushing the release.

.PARAMETER DryRun
  Validates the release and displays the proposed version without modifying
  files, creating a commit, creating a tag, or pushing anything.

.EXAMPLE
  .\scripts\release.ps1 -Bump Patch

.EXAMPLE
  .\scripts\release.ps1 -Bump Minor

.EXAMPLE
  .\scripts\release.ps1 -Bump Major

.EXAMPLE
  .\scripts\release.ps1 -Bump Minor -DryRun
#>

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [ValidateSet("Patch", "Minor", "Major")]
    [string]$Bump,

    [string]$ProjectPath =
    "src/EnumerablePrinter/EnumerablePrinter.csproj",

    [string]$SolutionPath = "EnumerablePrinter.sln",

    [string]$Branch = "main",

    [string]$Remote = "origin",

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

    $command = Get-Command $CommandName -ErrorAction SilentlyContinue

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
            throw "A Git $($marker.Name) operation is in progress. Complete or abort it before releasing."
        }
    }

    Write-Log "Git has no merge, rebase, cherry-pick, revert, or bisect operation in progress."
}

function Assert-GitIdentity {
    $userName = Get-NativeCommandOutput `
        -Command {
        git config --get user.name
    } `
        -FailureMessage "Git user.name is not configured."

    $userEmail = Get-NativeCommandOutput `
        -Command {
        git config --get user.email
    } `
        -FailureMessage "Git user.email is not configured."

    $userName =
    ($userName | Select-Object -First 1).Trim()

    $userEmail =
    ($userEmail | Select-Object -First 1).Trim()

    if ([string]::IsNullOrWhiteSpace($userName)) {
        throw "Git user.name is empty."
    }

    if ([string]::IsNullOrWhiteSpace($userEmail)) {
        throw "Git user.email is empty."
    }

    if ($userEmail -notmatch '^[^@\s]+@[^@\s]+\.[^@\s]+$') {
        throw "Git user.email '$userEmail' does not appear to be valid."
    }

    Write-Log "Git author: $userName <$userEmail>"
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

    if ([string]::IsNullOrWhiteSpace($remoteUrl)) {
        throw "Git remote '$Remote' has no URL."
    }

    Write-Log "Remote '$Remote': $remoteUrl"
}

function Assert-ExpectedBranch {
    $currentBranch = Get-NativeCommandOutput `
        -Command {
        git symbolic-ref --quiet --short HEAD
    } `
        -FailureMessage "Detached HEAD detected. Check out branch '$Branch' before releasing."

    $currentBranch =
    ($currentBranch | Select-Object -First 1).Trim()

    if ($currentBranch -ne $Branch) {
        throw "Release must run from branch '$Branch'. Current branch is '$currentBranch'."
    }

    Write-Log "Branch '$Branch' is checked out."
}

function Assert-CleanWorkingTree {
    $status = Get-NativeCommandOutput `
        -Command {
        git status --porcelain
    } `
        -FailureMessage "Unable to inspect the Git working tree."

    if ($status) {
        throw "The working tree is not clean. Commit or stash all changes before releasing."
    }

    Write-Log "Working tree is clean."
}

function Assert-RepositoryState {
    Write-Log "Validating Git repository state..."

    Assert-NoGitOperationInProgress
    Assert-RemoteExists
    Assert-GitIdentity
    Assert-ExpectedBranch
    Assert-CleanWorkingTree
}

function Sync-And-ValidateBranch {
    Write-Log "Fetching '$Remote'..."

    Invoke-NativeCommand `
        -Command {
        git fetch --prune --tags $Remote
    } `
        -FailureMessage "Unable to fetch from remote '$Remote'."

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
        -FailureMessage "Unable to determine the local HEAD commit."

    $remoteCommit = Get-NativeCommandOutput `
        -Command {
        git rev-parse "$Remote/$Branch"
    } `
        -FailureMessage "Unable to determine the '$Remote/$Branch' commit."

    $localCommit =
    ($localCommit | Select-Object -First 1).Trim()

    $remoteCommit =
    ($remoteCommit | Select-Object -First 1).Trim()

    if ($localCommit -ne $remoteCommit) {
        $counts = Get-NativeCommandOutput `
            -Command {
            git rev-list `
                --left-right `
                --count `
                "$Remote/$Branch...HEAD"
        } `
            -FailureMessage "Unable to compare local and remote branch history."

        $countText =
        ($counts | Select-Object -First 1).Trim()

        $parts =
        $countText -split '\s+'

        if ($parts.Count -lt 2) {
            throw "Unable to interpret the local and remote branch comparison."
        }

        $behind = $parts[0]
        $ahead = $parts[1]

        throw "Local '$Branch' does not match '$Remote/$Branch'. Ahead: $ahead; behind: $behind. Synchronize the branch before releasing."
    }

    Write-Log "Local HEAD matches '$Remote/$Branch'."
}

function Get-VersionFilePath {
    return Join-Path $script:RepositoryRoot "VERSION"
}

function Get-FullProjectPath {
    return Join-Path $script:RepositoryRoot $ProjectPath
}

function Get-FullSolutionPath {
    return Join-Path $script:RepositoryRoot $SolutionPath
}

function Get-Version {
    $versionFile = Get-VersionFilePath

    if (-not (Test-Path $versionFile)) {
        throw "VERSION file not found."
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
    $fullProjectPath = Get-FullProjectPath

    if (-not (Test-Path $fullProjectPath)) {
        throw "Project file not found: $ProjectPath"
    }

    $content =
    [System.IO.File]::ReadAllText($fullProjectPath)

    $matches =
    [System.Text.RegularExpressions.Regex]::Matches(
        $content,
        '<Version>\s*([^<]+?)\s*</Version>')

    if ($matches.Count -ne 1) {
        throw "Expected exactly one <Version> element in $ProjectPath, but found $($matches.Count)."
    }

    $version =
    $matches[0].Groups[1].Value.Trim()

    if ($version -notmatch '^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$') {
        throw "Project version '$version' is not a valid major.minor.patch version."
    }

    return $version
}

function Assert-VersionFilesMatch {
    $fileVersion = Get-Version
    $projectVersion = Get-ProjectVersion

    if ($fileVersion -ne $projectVersion) {
        throw "VERSION contains '$fileVersion', but $ProjectPath contains '$projectVersion'."
    }

    Write-Log "VERSION and the project file both contain '$fileVersion'."
}

function Get-NextVersion {
    param(
        [Parameter(Mandatory)]
        [string]$CurrentVersion,

        [Parameter(Mandatory)]
        [ValidateSet("Patch", "Minor", "Major")]
        [string]$BumpType
    )

    $segments =
    $CurrentVersion.Split(".")

    [int]$major = $segments[0]
    [int]$minor = $segments[1]
    [int]$patch = $segments[2]

    switch ($BumpType) {
        "Patch" {
            $patch++
        }

        "Minor" {
            $minor++
            $patch = 0
        }

        "Major" {
            $major++
            $minor = 0
            $patch = 0
        }
    }

    return "$major.$minor.$patch"
}

function Assert-TagDoesNotExist {
    param(
        [Parameter(Mandatory)]
        [string]$Tag
    )

    $localTag = Get-NativeCommandOutput `
        -Command {
        git tag --list $Tag
    } `
        -FailureMessage "Unable to inspect local Git tags."

    if ($localTag) {
        throw "Local tag '$Tag' already exists."
    }

    $remoteTag = Get-NativeCommandOutput `
        -Command {
        git ls-remote `
            --tags `
            $Remote `
            "refs/ags/$Tag^{}"
    } `
        -FailureMessage "Unable to inspect tags on remote '$Remote'."

    if ($remoteTag) {
        throw "Tag '$Tag' already exists on remote '$Remote'."
    }

    Write-Log "Tag '$Tag' is available."
}

function Set-VersionFile {
    param(
        [Parameter(Mandatory)]
        [string]$Version
    )

    $versionFile = Get-VersionFilePath

    Write-Log "Updating VERSION to '$Version'..."

    [System.IO.File]::WriteAllText(
        $versionFile,
        $Version,
        [System.Text.UTF8Encoding]::new($false))
}

function Update-CsprojVersion {
    param(
        [Parameter(Mandatory)]
        [string]$Version
    )

    $fullProjectPath = Get-FullProjectPath

    Write-Log "Updating project version to '$Version'..."

    $content =
    [System.IO.File]::ReadAllText($fullProjectPath)

    $matches =
    [System.Text.RegularExpressions.Regex]::Matches(
        $content,
        '<Version>[^<]*</Version>')

    if ($matches.Count -ne 1) {
        throw "Expected exactly one <Version> element in $ProjectPath, but found $($matches.Count)."
    }

    $updatedContent =
    [System.Text.RegularExpressions.Regex]::Replace(
        $content,
        '<Version>[^<]*</Version>',
        "<Version>$Version</Version>",
        1)

    [System.IO.File]::WriteAllText(
        $fullProjectPath,
        $updatedContent,
        [System.Text.UTF8Encoding]::new($false))
}

function Restore-VersionFiles {
    Write-Log "Restoring VERSION and the project file..."

    & git restore -- VERSION $ProjectPath

    if ($LASTEXITCODE -ne 0) {
        Write-Log "WARNING: Automatic restoration failed."
        Write-Log "Inspect VERSION and '$ProjectPath' manually."
        return
    }

    Write-Log "Version files restored."
}

function Invoke-ReleaseChecks {
    $fullSolutionPath = Get-FullSolutionPath

    if (-not (Test-Path $fullSolutionPath)) {
        throw "Solution file not found: $SolutionPath"
    }

    Write-Log "Running release build and tests..."

    Invoke-NativeCommand `
        -Command {
        dotnet test $fullSolutionPath `
            --configuration Release `
            --nologo
    } `
        -FailureMessage "Release build or tests failed."

    Write-Log "Release build and tests passed."
}

function Assert-ExpectedStagedFiles {
    $stagedFiles = @(
        Get-NativeCommandOutput `
            -Command {
            git diff --cached --name-only
        } `
            -FailureMessage "Unable to inspect staged release files."
    )

    $expectedFiles = @(
        "VERSION",
        $ProjectPath.Replace("\", "/")
    )

    $normalizedStagedFiles = @(
        $stagedFiles |
        ForEach-Object {
            $_.Trim().Replace("\", "/")
        } |
        Where-Object {
            -not [string]::IsNullOrWhiteSpace($_)
        }
    )

    foreach ($stagedFile in $normalizedStagedFiles) {
        if ($stagedFile -notin $expectedFiles) {
            throw "Unexpected staged file '$stagedFile'. Only VERSION and '$ProjectPath' may be included in the release commit."
        }
    }

    foreach ($expectedFile in $expectedFiles) {
        if ($expectedFile -notin $normalizedStagedFiles) {
            throw "Expected release file '$expectedFile' is not staged."
        }
    }

    Write-Log "Only the expected release files are staged."
}

function Assert-TagPointsToHead {
    param(
        [Parameter(Mandatory)]
        [string]$Tag
    )

    $tagCommit = Get-NativeCommandOutput `
        -Command {
        git rev-list -n 1 $Tag
    } `
        -FailureMessage "Unable to resolve tag '$Tag'."

    $headCommit = Get-NativeCommandOutput `
        -Command {
        git rev-parse HEAD
    } `
        -FailureMessage "Unable to resolve the release commit."

    $tagCommit =
    ($tagCommit | Select-Object -First 1).Trim()

    $headCommit =
    ($headCommit | Select-Object -First 1).Trim()

    if ($tagCommit -ne $headCommit) {
        throw "Tag '$Tag' does not point to the release commit."
    }

    Write-Log "Tag '$Tag' points to the release commit."
}

Assert-CommandExists -CommandName "git"
Assert-CommandExists -CommandName "dotnet"

$originalLocation = Get-Location
$versionFilesChanged = $false
$releaseCommitted = $false
$tagCreated = $false
$createdTag = $null

try {
    $script:RepositoryRoot = Get-RepositoryRoot
    Set-Location $script:RepositoryRoot

    Write-Log "Starting release validation..."
    Write-Log "Repository: $script:RepositoryRoot"

    Assert-RepositoryState
    Sync-And-ValidateBranch
    Assert-VersionFilesMatch

    $currentVersion = Get-Version

    $newVersion = Get-NextVersion `
        -CurrentVersion $currentVersion `
        -BumpType $Bump

    $tag = "v$newVersion"
    $createdTag = $tag

    Write-Log "Current version: $currentVersion"
    Write-Log "Bump type: $Bump"
    Write-Log "New version: $newVersion"
    Write-Log "Release tag: $tag"

    Assert-TagDoesNotExist -Tag $tag

    # Run all non-destructive release validation before deciding
    # whether this is a dry run.
    Invoke-ReleaseChecks

    if ($DryRun) {
        Write-Log "Dry run enabled."
        Write-Log "Git, version, tag, build, and test checks passed."
        Write-Log "Proposed release: $currentVersion -> $newVersion"
        Write-Log "Proposed tag: $tag"
        Write-Log "No files, commits, tags, branches, or remotes were changed."
        Write-Log "Release validation complete."
        return
    }

    Set-VersionFile -Version $newVersion
    Update-CsprojVersion -Version $newVersion
    $versionFilesChanged = $true

    Assert-VersionFilesMatch

    Write-Log "Staging release version files..."

    Invoke-NativeCommand `
        -Command {
        git add -- VERSION $ProjectPath
    } `
        -FailureMessage "Unable to stage release files."

    Assert-ExpectedStagedFiles

    Write-Log "Committing release '$newVersion'..."

    Invoke-NativeCommand `
        -Command {
        git commit -m "Release $newVersion"
    } `
        -FailureMessage "Unable to commit release '$newVersion'."

    $releaseCommitted = $true
    $versionFilesChanged = $false

    Write-Log "Creating annotated tag '$tag'..."

    Invoke-NativeCommand `
        -Command {
        git tag -a $tag -m "Release $newVersion"
    } `
        -FailureMessage "Unable to create tag '$tag'."

    $tagCreated = $true

    Assert-TagPointsToHead -Tag $tag

    Write-Log "Pushing release commit to '$Remote/$Branch'..."

    Invoke-NativeCommand `
        -Command {
        git push --porcelain $Remote "HEAD:$Branch"
    } `
        -FailureMessage "Unable to push branch '$Branch'. The release commit remains local."

    Write-Log "Pushing release tag '$tag'..."

    Invoke-NativeCommand `
        -Command {
        git push --porcelain $Remote "refs/tags/$tag"
    } `
        -FailureMessage "The release commit was pushed, but tag '$tag' could not be pushed."

    Write-Log "Release '$newVersion' created successfully."
    Write-Log "Next run: .\scripts\deploy.ps1 -DryRun"
}
catch {
    Write-Log "ERROR: $($_.Exception.Message)"

    if ($versionFilesChanged -and -not $releaseCommitted) {
        Restore-VersionFiles
    }

    if ($tagCreated -and -not $releaseCommitted -and $createdTag) {
        Write-Log "Removing incomplete release tag '$createdTag'..."
        git tag -d $createdTag 2>$null
    }

    if ($releaseCommitted) {
        Write-Log "WARNING: A release commit was created locally."
        Write-Log "Inspect 'git status', 'git log -1', and 'git tag --list' before taking further action."
        Write-Log "Do not rerun this release command blindly."
    }

    exit 1
}
finally {
    Set-Location $originalLocation
}