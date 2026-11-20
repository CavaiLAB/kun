#!/usr/bin/env bash
# Usage: bash scripts/release.sh <version>
#   CalVer with NuGet-compatible labels (YYYY.M.D[-hotfix.N|-beta.N],
#   month/day not zero-padded, always cut from main):
#   e.g. bash scripts/release.sh 2026.6.12
#        bash scripts/release.sh 2026.6.12-hotfix.2
#        bash scripts/release.sh 2026.6.12-beta.1
#
# Local pre-flight for a candidate version. Builds the solution, runs its unit
# tests, and publishes/runs the NativeAOT smoke host on Linux with a compiler.
# Publish the verified main commit via the release workflow in GitHub Actions.
#
# Windows: run via Git Bash ("bash scripts/release.sh ...") or WSL.

set -euo pipefail

# ---------------------------------------------------------------------------
# Args
# ---------------------------------------------------------------------------
VERSION="${1:?Usage: bash scripts/release.sh <version>  (e.g. 2026.6.12 or 2026.6.12-beta.1)}"
TAG="v$VERSION"

log()  { printf '\n\033[1;34m▶ %s\033[0m\n' "$*"; }
warn() { printf '\033[1;33m⚠ %s\033[0m\n' "$*" >&2; }
err()  { printf '\033[1;31m✗ %s\033[0m\n' "$*" >&2; }

# ---------------------------------------------------------------------------
# Phase 0: gates
# ---------------------------------------------------------------------------
log "Phase 0/3 · Gates (calver / branch / clean tree / tag-absence)"

# The same validator is used by CI, including the calendar and NuGet suffix constraints.
python3 "$(dirname "${BASH_SOURCE[0]}")/release_version.py" --version "$VERSION"

BRANCH=$(git rev-parse --abbrev-ref HEAD)
if [[ "$BRANCH" != "main" && ! "$BRANCH" =~ ^hotfix/ ]]; then
  err "releases must be cut from 'main' (or a 'hotfix/*' branch for old-version patches); current branch: '$BRANCH'"
  exit 1
fi

if ! git diff --quiet; then
  err "working tree has unstaged changes — commit or stash them first"
  exit 1
fi
if ! git diff --cached --quiet; then
  err "index has staged changes — commit or stash them first"
  exit 1
fi

if git rev-parse "$TAG" >/dev/null 2>&1; then
  err "tag '$TAG' already exists locally — delete it first with: git tag -d $TAG"
  exit 1
fi

# ---------------------------------------------------------------------------
# Phase 1: build
# ---------------------------------------------------------------------------
log "Phase 1/3 · dotnet build -c Release /warnaserror"
dotnet build -c Release -p:Version="$VERSION" /warnaserror

# ---------------------------------------------------------------------------
# Phase 2: tests
# ---------------------------------------------------------------------------
log "Phase 2/3 · dotnet test (unit only; IntegrationTests / Sample.Tests / Integration.AotTests left out)"
# Microsoft.Testing.Platform (global.json test.runner): the unit projects are listed in
# tests/kun-unit.slnf, which leaves out the same three projects the old VSTest filter excluded.
dotnet test --solution tests/kun-unit.slnf -c Release --no-build

# ---------------------------------------------------------------------------
# Phase 3: AOT smoke host
# ---------------------------------------------------------------------------
# The AOT list has a single source: projects whose csproj sets
# IsAotCompatible=true, listed by scripts/aot_projects.py. tests/Kun.AotSmoke
# references every one of them and roots each assembly whole
# (TrimmerRootAssembly), so ILC analyses all of their code. The steps below are
# the ones the CI aot-smoke job in .github/workflows/ci.yml runs.
#
# This phase used to `dotnet publish -p:PublishAot=true` each library in the
# list instead. A library publishes as a native shared library with no ILC
# roots, so none of its own code was analysed; and the command-line PublishAot
# flows into every project reference, which fails restore for the
# netstandard2.0 analyzer projects (NETSDK1207) and turns the AOT analyzers on
# in dependencies that are not AOT-compatible.
log "Phase 3/3 · AOT smoke host (tests/Kun.AotSmoke)"

if [[ "$(uname -s)" != Linux ]]; then
  warn "The linux-x64 NativeAOT smoke host runs on the release runner."
elif ! command -v clang >/dev/null 2>&1 && ! command -v gcc >/dev/null 2>&1; then
  warn "No clang / gcc on PATH — skipping the AOT smoke host."
  warn "The CI AOT smoke host job (tests/Kun.AotSmoke) is the authoritative AOT gate."
  warn "Install build-essential + clang + zlib1g-dev to run this phase locally."
else
  python3 "$(dirname "${BASH_SOURCE[0]}")/aot_projects.py" --check
  AOT_SMOKE_OUT="$(mktemp -d "${TMPDIR:-/tmp}/kun-aot-smoke.XXXXXX")"
  dotnet publish tests/Kun.AotSmoke/Kun.AotSmoke.csproj -c Release -r linux-x64 \
    -p:Version="$VERSION" -p:IlcTreatWarningsAsErrors=true -p:TrimmerSingleWarn=false \
    -o "$AOT_SMOKE_OUT" --verbosity minimal --nologo < /dev/null
  if [ -e "$AOT_SMOKE_OUT/Kun.AotSmoke.dll" ]; then
    err "tests/Kun.AotSmoke was not published as NativeAOT: its csproj must set PublishAot"
    exit 1
  fi
  "$AOT_SMOKE_OUT/Kun.AotSmoke"
  rm -rf "$AOT_SMOKE_OUT"
fi

log "Local validation completed for $VERSION."
printf 'Run the release workflow on main with version %s and dry_run=true to prepare and verify the complete bundle.\n' "$VERSION"
