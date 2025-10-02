param(
    [string]$Version = "1.0.3",
    [string]$ApiKey  = $env:NUGET_API_KEY,
    [string]$CommitMessage = "Release $Version"
)

$projectPath = "src/EnumerablePrinter/EnumerablePrinter.csproj"
$packageDir  = "src/EnumerablePrinter/bin/Release"

Write-Host "=== Checking in changes to Git ==="

# Stage everything
git add .

# Commit if there are staged changes
$changes = git diff --cached --name-only
if ($changes) {
    git commit -m $CommitMessage
    git push origin main
} else {
    Write-Host "No changes to commit."
}

# Tag the release
$tag = "v$Version"
if (-not (git tag -l $tag)) {
    git tag $tag
    git push origin $tag
    Write-Host "Created and pushed tag $tag"
} else {
    Write-Host "Tag $tag already exists, skipping."
}

Write-Host "=== Building and Packing EnumerablePrinter v$Version ==="

dotnet clean $projectPath -c Release
dotnet build $projectPath -c Release
dotnet test tests/EnumerablePrinter.Tests/EnumerablePrinter.Tests.csproj -c Release

dotnet pack $projectPath -c Release -p:PackageVersion=$Version

$package = Join-Path $packageDir "EnumerablePrinter.$Version.nupkg"
$symbols = Join-Path $packageDir "EnumerablePrinter.$Version.snupkg"

if (-Not (Test-Path $package)) {
    Write-Error "Package file not found: $package"
    exit 1
}

Write-Host "=== Pushing to NuGet.org ==="

dotnet nuget push $package `
    --api-key $ApiKey `
    --source https://api.nuget.org/v3/index.json `
    --skip-duplicate

if (Test-Path $symbols) {
    dotnet nuget push $symbols `
        --api-key $ApiKey `
        --source https://api.nuget.org/v3/index.json `
        --skip-duplicate
}

Write-Host "=== Deployment Complete ==="
