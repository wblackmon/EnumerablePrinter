#!/bin/bash

# ┌────────────────────────────────────────────────────────────┐
# │  EnumerablePrinter Release Script (Linux/macOS)            │
# │                                                            │
# │  Bumps version, updates csproj, commits, and tags.        │
# │  This script does not package or publish to NuGet.          │
# │                                                            │
# │  Usage:                                                    │
# │    ./release.sh                                             │
# │    ./release.sh --dry-run                                   │
# └────────────────────────────────────────────────────────────┘

PROJECT_PATH="src/EnumerablePrinter/EnumerablePrinter.csproj"
DRY_RUN=false

for arg in "$@"; do
  case "$arg" in
    --dry-run)
      DRY_RUN=true
      ;;
  esac
done

log() {
  echo "$(date '+%Y-%m-%d %H:%M:%S') - $1"
}

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
  echo "$new_version"
}

update_csproj_version() {
  version="$1"
  log "📝 Updating .csproj version to $version"
  sed -i.bak "s|<Version>.*</Version>|<Version>$version</Version>|" "$PROJECT_PATH"
  rm "$PROJECT_PATH.bak"
}

log "🚀 Starting release bump..."
new_version=$(bump_version)
tag="v$new_version"

log "🔢 New version: $new_version"
log "🏷️ Tag to be created: $tag"

if [ "$DRY_RUN" = true ]; then
  log "🧪 Dry run enabled. Skipping file updates, commit, tag, and push."
  log "✅ Release bump validation complete."
  exit 0
fi

set_version "$new_version"
update_csproj_version "$new_version"

log "📦 Staging version bump..."
git add VERSION
if [ -f "$PROJECT_PATH" ]; then
  git add "$PROJECT_PATH"
fi

log "📝 Committing version bump..."
git commit -m "Release $new_version"

log "🏷️ Tagging release..."
git tag "$tag"

git push origin main
git push origin "$tag"

log "✅ Release bump complete."
