#!/bin/bash

# ┌────────────────────────────────────────────────────────────┐
# │  EnumerablePrinter Deploy Script (Linux/macOS)             │
# │                                                            │
# │  Builds, tests, packs, and pushes the current package to   │
# │  NuGet.org. This script assumes the version has already    │
# │  been bumped and tagged via the release script.            │
# │                                                            │
# │  Requires:                                                 │
# │    - nuget.secret (git-ignored) OR $NUGET_API_KEY          │
# │                                                            │
# │  Usage:                                                    │
# │    ./deploy.sh                                             │
# │    ./deploy.sh --dry-run                                   │
# └────────────────────────────────────────────────────────────┘

set -euo pipefail

PROJECT_PATH="src/EnumerablePrinter/EnumerablePrinter.csproj"
TEST_PROJECT_PATH="tests/EnumerablePrinter.Tests/EnumerablePrinter.Tests.csproj"
OUTPUT_DIR="nupkg"
DRY_RUN=false

for arg in "$@"; do
  case "$arg" in
    --dry-run)
      DRY_RUN=true
      ;;
    --help|-h)
      echo "Usage: ./deploy.sh [--dry-run]"
      exit 0
      ;;
    *)
      echo "Unknown argument: $arg"
      echo "Usage: ./deploy.sh [--dry-run]"
      exit 1
      ;;
  esac
done

log() {
  echo "$(date '+%Y-%m-%d %H:%M:%S') - $1"
}

load_nuget_key() {
  if [ -f "nuget.secret" ]; then
    log "🔐 Loading NuGet API key from nuget.secret"
    # shellcheck source=/dev/null
    source nuget.secret
    if [ -n "${NuGetApiKey:-}" ]; then
      export NUGET_API_KEY="$NuGetApiKey"
    fi
  fi

  if [ -z "${NUGET_API_KEY:-}" ]; then
    echo "❌ NuGet API key not set. Create a file named 'nuget.secret' with:"
    echo "NuGetApiKey=your-key-here"
    exit 1
  fi
}

package_exists() {
  local version="$1"
  local url="https://api.nuget.org/v3-flatcontainer/enumerableprinter/$version/enumerableprinter.$version.nupkg"
  curl -fsSL --head "$url" >/dev/null 2>&1
}

build_and_pack() {
  local version="$1"

  log "🔧 Restoring project..."
  dotnet restore "$PROJECT_PATH"

  log "🛠️ Building project..."
  dotnet build "$PROJECT_PATH" -c Release

  log "🧪 Running tests..."
  dotnet test "$TEST_PROJECT_PATH" --no-build

  log "📦 Packing version $version..."
  dotnet pack "$PROJECT_PATH" -c Release -o "$OUTPUT_DIR" "-p:PackageVersion=$version"
}

push_to_nuget() {
  local version="$1"
  local package="$OUTPUT_DIR/EnumerablePrinter.$version.nupkg"

  if [ ! -f "$package" ]; then
    echo "❌ Package not found: $package"
    exit 1
  fi

  log "📦 Found package: $(basename "$package")"

  if [ "$DRY_RUN" = true ]; then
    log "🧪 Dry run enabled. Skipping NuGet push."
    return
  fi

  log "📤 Pushing to NuGet..."
  dotnet nuget push "$package" \
    --api-key "$NUGET_API_KEY" \
    --source https://api.nuget.org/v3/index.json
}

main() {
  if [ ! -f "$PROJECT_PATH" ]; then
    echo "❌ Project file not found: $PROJECT_PATH"
    exit 1
  fi

  if [ ! -f "$TEST_PROJECT_PATH" ]; then
    echo "❌ Test project file not found: $TEST_PROJECT_PATH"
    exit 1
  fi

  if [ ! -f "VERSION" ]; then
    echo "❌ VERSION file not found. Run the release script first."
    exit 1
  fi

  version=$(tr -d '\r\n' < VERSION)
  if [ -z "$version" ]; then
    echo "❌ VERSION file is empty. Run the release script first."
    exit 1
  fi

  load_nuget_key

  if package_exists "$version"; then
    log "⚠️ Version $version already exists on NuGet. Skipping push."
    exit 0
  fi

  log "🚀 Starting deploy..."
  build_and_pack "$version"
  push_to_nuget "$version"

  log "✅ Deploy complete."
}

main "$@"
