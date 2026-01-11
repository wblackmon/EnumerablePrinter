#!/bin/bash

# ┌────────────────────────────────────────────────────────────┐
# │  EnumerablePrinter Check‑In Script                         │
# │                                                            │
# │  Usage:                                                    │
# │    ./checkin.sh "Fix bug"                                  │
# │                                                            │
# │  Args:                                                     │
# │    $1 - Commit message (default: "Update EnumerablePrinter") │
# └────────────────────────────────────────────────────────────┘

MESSAGE="${1:-Update EnumerablePrinter}"

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

log "🔄 Rebasing from origin/main..."
git pull --rebase origin main

log "🚀 Pushing to origin/main..."
git push origin main

log "✅ Check‑in complete (no version bump)."
