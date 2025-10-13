#!/bin/bash

# ┌────────────────────────────────────────────────────────────┐
# │  EnumerablePrinter Update Script                           │
# │                                                            │
# │  Usage:                                                    │
# │    ./update.sh "Fix CS8602 warning" v1.0.5 true            │
# │                                                            │
# │  Args:                                                     │
# │    $1 - Commit message (default: "Update EnumerablePrinter") │
# │    $2 - Optional Git tag (e.g., v1.0.5)                     │
# │    $3 - Deploy flag (true/false)                           │
# └────────────────────────────────────────────────────────────┘

MESSAGE="${1:-Update EnumerablePrinter}"
TAG="$2"
DEPLOY="${3:-false}"

log() {
  echo "$(date '+%Y-%m-%d %H:%M:%S') - $1"
}

log "🔍 Running build and test checks..."
dotnet build --nologo --verbosity quiet || { echo "❌ Build failed."; exit 1; }
dotnet test --no-build --nologo --verbosity quiet || { echo "❌ Tests failed."; exit 1; }
log "✅ Checks passed."

log "📦 Staging changes..."
git add .

log "📝 Committing: '$MESSAGE'"
git commit -m "$MESSAGE"

log "🚀 Pushing to origin/main..."
git push origin main

if [ -n "$TAG" ]; then
  log "🏷️ Tagging release: $TAG"
  git tag "$TAG"
  git push origin "$TAG"
fi

if [ "$DEPLOY" = "false" ]; then
  read -p "📤 Do you want to deploy this version now? (y/n): " response
  if [[ "$response" =~ ^[Yy] ]]; then
    DEPLOY="true"
  fi
fi

if [ "$DEPLOY" = "true" ]; then
  log "📤 Running deploy script..."
  ./deploy.ps1 -Version "$TAG"
fi

log "✅ Update complete."