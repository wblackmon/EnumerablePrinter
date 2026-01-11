#!/bin/bash

# ┌────────────────────────────────────────────────────────────┐
# │  EnumerablePrinter Deploy Script (Linux/macOS)             │
# │                                                            │
# │  Bumps version, updates csproj, commits, tags, builds,     │
# │  tests, packs, and pushes to NuGet.org.                    │
# │                                                            │
# │  Requires:                                                 │
# │    - nuget.secret (git‑ignored) OR $NUGET_API_KEY          │
# │                                                            │
# │  Usage:                                                    │
# │    ./deploy.sh                                             │
# │    ./deploy.sh --dry-run                                   │
# └────────────────────────────────────────────────────────────┘

PROJECT_PATH="src/EnumerablePrinter/EnumerablePrinter.csproj"
TEST_PROJECT_PATH="tests/EnumerablePrinter.Tests/EnumerablePrinter.Tests.csproj"
OUTPUT_DIR="nupkg"
DRY_RUN=false

# Parse args
for arg in "$@"; do
  case $arg in
    --dry-run)
      DRY_RUN=true
      ;;
  esac
done

log() {
  echo "$(date '+%Y-%m-%d %H:%M:%S') - $1"
}

# Initialize to satisfy ShellCheck (prevents “referenced but not assigned”)
NuGetApiKey=""

# Load NuGet API key from nuget.secret if present
if [ -f "nuget.secret" ]; then
  log "🔐 Loading NuGet API key from nuget.secret"
  # shellcheck source=/dev/null
  source nuget.secret
  export NUGET_API_KEY="$NuGetApiKey"
fi

# Ensure API key exists
if [ -z "$NUGET_API_KEY" ]; then
  echo "❌ NuGet API key not set. Create a file named 'nuget.secret' with:"
  echo "NuGetApiKey=your-key-here"
  exit 1
fi

get_version() {
  if [ ! -f VERSION ]; then
    echo "VERSION file not found."
    exit 1
  fi
  cat VERSION
}

set_version() {
  echo -n "$1" > VERSION
}

bump_version() {
  version=$(get_version)
  IFS='.' read -r major minor patch <<< "$version"

  patch=$((patch + 1))

  if [ "$patch" -gt 9 ]; then
    patch=0
    minor=$((minor + 1))
  fi

  if [ "$minor" -gt 9 ]; then
    minor=0
    major=$((major + 1))
  fi

  new_version="$major.$minor.$patch"
  set_version "$new_version"
  echo "$new_version"
}

update_csproj_version() {
  version="$1"
  log "📝 Updating .csproj version to $version"

  sed -i.bak "s|<Version>.*</Version>|<Version>$version</Version>|" "$PROJECT_PATH"
  rm "$PROJECT_PATH.bak"
}

package_exists() {
  version="$1"
  url="https://api.nuget.org/v3-flatcontainer/enumerableprinter/$version/enumerableprinter.$version.nupkg"
  curl -s --head "$url" | head -n 1 | grep "200 OK" > /dev/null
}

build_and_pack() {
  version="$1"

  log "🔧 Restoring project..."
  dotnet restore "$PROJECT_PATH"

  log "🛠️ Building project..."
  dotnet build "$PROJECT_PATH" -c Release

  log "🧪 Running tests..."
  dotnet test "$TEST_PROJECT_PATH" --no-build

  log "📦 Packing version $version..."
  dotnet pack "$PROJECT_PATH" -c Release -o "$OUTPUT_DIR" /p:PackageVersion="$version"
}

push_to_nuget() {
  version="$1"
  package="$OUTPUT_DIR/EnumerablePrinter.$version.nupkg"

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

# MAIN
log "🚀 Starting deploy..."

# 1. Bump version
new_version=$(bump_version)
tag="v$new_version"

log "🔢 New version: $new_version"
log "🏷️ Tag to be created: $tag"

# 2. Update .csproj
update_csproj_version "$new_version"

# 3. Commit version bump
log "📦 Staging version bump..."
git add VERSION
git add "$PROJECT_PATH"

log "📝 Committing version bump..."
git commit -m "Release $new_version"

# 4. Tag release
log "🏷️ Tagging release..."
git tag "$tag"
git push origin main
git push origin "$tag"

# 5. Skip if version already exists
if package_exists "$new_version"; then
  log "⚠️ Version $new_version already exists on NuGet. Skipping push."
  exit 0
fi

# 6. Build, test, pack, push
build_and_pack "$new_version"
push_to_nuget "$new_version"

log "✅ Deploy complete."
