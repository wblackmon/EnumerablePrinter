param (
    [string]$Version = "1.0.0",
    [string]$ProjectPath = "src/EnumerablePrinter/EnumerablePrinter.csproj",
    [string]$OutputDir = "nupkg"
)

function Write-Log {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "$timestamp - $Message"
}

function Ensure-NuGetApiKey {
    if (-not $env:NUGET_API_KEY) {
        throw "NUGET_API_KEY environment variable is not set."
    }
}

function Check-PackageExists {
    $url = "https://api.nuget.org/v3-flatcontainer/enumerableprinter/$Version/enumerableprinter.$Version.nupkg"
    try {
        $response = Invoke-WebRequest -Uri $url -Method Head -UseBasicParsing
        return $response.StatusCode -eq 200
    } catch {
        return $false
    }
}

function Build-And-Pack {
    Write-Log "Restoring project..."
    dotnet restore $ProjectPath

    Write-Log "Building project..."
    dotnet build $ProjectPath -c Release

    Write-Log "Running tests..."
    dotnet test tests/EnumerablePrinter.Tests/EnumerablePrinter.Tests.csproj --no-build

    Write-Log "Packing version $Version..."
    dotnet pack $ProjectPath -c Release -o $OutputDir /p:PackageVersion=$Version
}

function Push-ToNuGet {
    $packagePath = Get-ChildItem "$OutputDir\EnumerablePrinter.$Version.nupkg" -ErrorAction Stop
    Write-Log "Pushing $($packagePath.Name) to NuGet..."
    dotnet nuget push $packagePath.FullName --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
}

# Main
Write-Log "Starting deploy for version $Version"
Ensure-NuGetApiKey

if (Check-PackageExists) {
    Write-Log "Version $Version already exists on NuGet. Skipping push."
    exit 0
}

Build-And-Pack
Push-ToNuGet
Write-Log "Deploy complete."
