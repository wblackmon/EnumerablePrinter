# Scripts

This folder contains the repository automation for the release and publishing workflow.

## Purpose

The scripts are intentionally split by responsibility so each action is easy to understand and maintain:

- release scripts handle versioning, git tagging, and branch release coordination
- deploy scripts handle build, validation, packaging, and NuGet publication

## PowerShell

- `checkin.ps1` — build, test, commit, rebase, and push current changes
- release.ps1 — bump the library version, update the project metadata, commit the version change, and create a git tag
- deploy.ps1 — restore, build, run tests, pack, and publish the package to NuGet
- show-folder-tree.ps1 — print a quick folder tree for repo inspection

## Bash

- `../checkin.sh` — build, test, commit, rebase, and push current changes
- release.sh — equivalent release flow for Unix-like environments
- deploy.sh — equivalent publish flow for Unix-like environments

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
3. Run the deploy script to package and publish the version to NuGet.

## Dry-run usage

Both deploy scripts support a dry-run flag:

```bash
./deploy.sh --dry-run
```

```powershell
./deploy.ps1 --dry-run
```

This runs the validation and packaging flow without pushing to NuGet.

## Notes

- The version is read from the root VERSION file.
- The package version is expected to already be set before deployment runs.
- Release and deploy are intentionally separate so the publish step is not coupled to git release operations.
