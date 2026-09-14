#!/bin/bash

set -euo pipefail

TEST_PROJECT_PATH="tests/EnumerablePrinter.Tests/EnumerablePrinter.Tests.csproj"
MESSAGE="Update EnumerablePrinter"
DRY_RUN=false

for arg in "$@"; do
  case "$arg" in
    --dry-run)
      DRY_RUN=true
      ;;
    --help|-h)
      echo "Usage: ./checkin.sh [--dry-run] [commit message]"
      exit 0
      ;;
    *)
      MESSAGE="$arg"
      ;;
  esac
done

log() {
  echo "$(date '+%Y-%m-%d %H:%M:%S') - $1"
}

log "🔍 Running build and test checks..."
dotnet build "$TEST_PROJECT_PATH" --nologo --verbosity quiet
dotnet test "$TEST_PROJECT_PATH" --no-build --nologo --verbosity quiet
log "✅ Checks passed."

if [ "$DRY_RUN" = true ]; then
  log "🧪 Dry run enabled. Skipping stage, commit, pull, and push."
  exit 0
fi

log "📦 Staging changes..."
git add .

if git diff --cached --quiet; then
  log "ℹ️ No changes to commit."
  exit 0
fi

log "📝 Committing: '$MESSAGE'"
git commit -m "$MESSAGE"

log "🔄 Rebasing from origin/main..."
git pull --rebase origin main

log "🚀 Pushing to origin/main..."
git push origin main

log "✅ Check-in complete."