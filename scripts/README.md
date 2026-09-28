# Scripts

Run repository scripts from the repository root unless a command below says otherwise.

## Script Reference

### Check in changes

PowerShell:

```powershell
.\checkin.ps1 -Message "Describe the change" -DryRun
.\checkin.ps1 -Message "Describe the change"
```

Shell:

```bash
./checkin.sh --dry-run "Describe the change"
./checkin.sh "Describe the change"
```

Both scripts build and test the core test project. Without `-DryRun` / `--dry-run`,
they stage **all** working-tree changes, commit, rebase from `origin/main`, and push
to `origin/main`. Review `git status` first; use dry-run to run checks without Git
changes.

### Prepare a release

```powershell
.\scripts\release.ps1 -DryRun
.\scripts\release.ps1
```

```bash
./scripts/release.sh --dry-run
./scripts/release.sh
```

Dry-run previews the next version without changing files or Git state. A real run
bumps `VERSION` and the core project version, commits the version change, creates
and pushes the version tag, and pushes `main`. It does not build, test, or publish.
Run only when the working tree and branch are ready for a release.

### Build and publish locally

```powershell
.\scripts\deploy.ps1 -DryRun
.\scripts\deploy.ps1
```

```bash
./scripts/deploy.sh --dry-run
./scripts/deploy.sh
```

Deploy restores, builds, tests, and packs the version in `VERSION`. A dry-run
still performs validation and packaging but skips the NuGet push; it still
requires credentials because the scripts check for the key before running.
PowerShell accepts `NUGET_API_KEY` or `nuget.secret.ps1` containing
`@{ NuGetApiKey = '...' }`. Bash accepts `NUGET_API_KEY` or `nuget.secret`
containing `NuGetApiKey=...`. A deploy skips publishing if that version already
exists on NuGet.

### Inspect the folder tree

```powershell
.\show-folder-tree.ps1
```

Prints the repository tree while omitting generated folders such as `bin`, `obj`,
`.vs`, and `TestResults`. Run it from the repository root.

### Scaffold analyzer files

```powershell
.\scripts\dotnet-analyzer_scaffold.ps1
```

Bootstraps analyzer and analyzer-test source files, adds Roslyn test packages, and
adds a project reference. This is a one-time scaffold helper, not a routine build
script. The repository already has analyzer files and package references, so do
not rerun it unless intentionally rebuilding that setup; inspect its changes
before accepting them.

## Recommended release flow

1. Finish code and documentation changes; run tests and the `net472` compatibility smoke test.
2. Run the release script's dry-run and review the proposed version.
3. Run the release script when ready to commit and push the version/tag.
4. GitHub Actions publishes the tag to NuGet through Trusted Publishing.

## Trusted publishing

The `.github/workflows/publish.yml` workflow is registered with NuGet as the `GitHubActions` trusted publisher for `wblackmon/EnumerablePrinter`. It runs for version tags matching `v*.*.*` and uses GitHub's OIDC token with `publish_mode: trusted`; no NuGet API key or GitHub secret is required.

The local deploy scripts do not use GitHub's OIDC identity. They continue to require `NUGET_API_KEY` or the ignored `nuget.secret` file when publishing from a developer machine.

## Notes

- The version is read from the root VERSION file.
- The package version is expected to already be set before deployment runs.
- Release and deploy are intentionally separate so the publish step is not coupled to git release operations.
- The release scripts default to updating `src/EnumerablePrinter/EnumerablePrinter.csproj`; PowerShell also accepts `-ProjectPath`.
- The PowerShell deploy script accepts `-ProjectPath`, `-TestProjectPath`, and `-OutputDir` overrides.
