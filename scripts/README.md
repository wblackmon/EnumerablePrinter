# Scripts

This folder contains the repository automation for the release and publishing workflow.

## Purpose

The scripts are intentionally split by responsibility so each action is easy to understand and maintain:

- release scripts handle versioning, git tagging, and branch release coordination
- deploy scripts handle build, validation, packaging, and NuGet publication

## PowerShell

- `../checkin.ps1` — build, test, commit, rebase, and push current changes
- release.ps1 — bump the library version, update the project metadata, commit the version change, and create a git tag
- deploy.ps1 — restore, build, run tests, pack, and publish locally to NuGet with an API key
- `../show-folder-tree.ps1` — print a quick folder tree for repo inspection

## Bash

- `../checkin.sh` — build, test, commit, rebase, and push current changes
- release.sh — equivalent release flow for Unix-like environments
- deploy.sh — equivalent local publish flow for Unix-like environments using an API key

The check-in scripts accept a commit message and support a non-mutating dry run:

```powershell
.\checkin.ps1 -Message "Update formatter" -DryRun
```

```bash
./checkin.sh --dry-run "Update formatter"
```

## Recommended flow

1. Update code and docs for the release.
2. Run the release script to bump the version and create the tag.
3. Push the release tag. GitHub Actions runs `publish.yml` and publishes to NuGet using Trusted Publishing.

## Dry-run usage

Both deploy scripts support a dry-run flag:

```bash
./deploy.sh --dry-run
```

```powershell
./deploy.ps1 --dry-run
```

This runs the validation and packaging flow without pushing to NuGet.

## Trusted publishing

The `.github/workflows/publish.yml` workflow is registered with NuGet as the `GitHubActions` trusted publisher for `wblackmon/EnumerablePrinter`. It runs for version tags matching `v*.*.*` and uses GitHub's OIDC token with `publish_mode: trusted`; no NuGet API key or GitHub secret is required.

The local deploy scripts do not use GitHub's OIDC identity. They continue to require `NUGET_API_KEY` or the ignored `nuget.secret` file when publishing from a developer machine.

## Notes

- The version is read from the root VERSION file.
- The package version is expected to already be set before deployment runs.
- Release and deploy are intentionally separate so the publish step is not coupled to git release operations.
