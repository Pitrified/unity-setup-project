#!/usr/bin/env bash
# Tools/setup.sh
# One-time dev-machine setup for artificial-pi.
# Run from any directory; script resolves the repo root automatically.
#
# What it does:
#   1. Verifies required toolchain (git, git-lfs, adb, bundletool)
#   2. Runs `git lfs install` (installs LFS hooks globally)
#   3. Installs the pre-commit hook from Tools/git-hooks/pre-commit
#   4. Copies build.env.example -> ~/.config/artificial-pi/build.env (if example exists)

set -euo pipefail

GREEN='\033[0;32m'
YELLOW='\033[0;33m'
RED='\033[0;31m'
NC='\033[0m'

ok()   { echo -e "${GREEN}[setup]  OK    $*${NC}"; }
warn() { echo -e "${YELLOW}[setup]  WARN  $*${NC}"; }
fail() { echo -e "${RED}[setup]  FAIL  $*${NC}" >&2; exit 1; }
info() { echo -e "[setup]        $*"; }

# ── Resolve repo root ────────────────────────────────────────────────────────
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
info "Repo root: $REPO_ROOT"

# ── 1. Toolchain verification ────────────────────────────────────────────────
info ""
info "--- Verifying toolchain ---"

check_tool() {
    local cmd="$1"
    local min_hint="$2"
    if command -v "$cmd" &>/dev/null; then
        ok "$cmd found: $(command -v "$cmd")"
    else
        warn "$cmd not found. $min_hint"
    fi
}

# git (required)
if ! command -v git &>/dev/null; then
    fail "git is not installed. Install git >= 2.40."
fi
GIT_VERSION=$(git --version | awk '{print $3}')
ok "git $GIT_VERSION"

# git-lfs (required)
if ! command -v git-lfs &>/dev/null; then
    fail "git-lfs is not installed. Install git-lfs >= 3.4 (https://git-lfs.com)."
fi
LFS_VERSION=$(git lfs version | awk '{print $1}' | cut -d/ -f2)
ok "git-lfs $LFS_VERSION"

# adb (recommended for device testing)
# Aliases are not expanded in non-interactive scripts, so probe real locations:
#   1. Already on PATH (custom install or CI)
#   2. Unity Hub SDK path (the alias target on this machine)
ADB_UNITY_PATH="$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb"
if command -v adb &>/dev/null; then
    ok "adb found in PATH: $(command -v adb)"
elif [[ -x "$ADB_UNITY_PATH" ]]; then
    ok "adb found at Unity SDK path: $ADB_UNITY_PATH"
    warn "adb is not on PATH. Add this to your shell profile (or use the alias in .bash_aliases.local):"
    warn "  export PATH=\"\$PATH:$(dirname "$ADB_UNITY_PATH")\""
else
    warn "adb not found. Needed for device testing. It ships with the Unity Android Build Support module."
fi

# bundletool (recommended for AAB smoke-testing)
# On this machine bundletool is a shell alias: java -jar ~/Unity/bundletool-all.jar
# Scripts cannot use aliases, so check for the jar directly.
BUNDLETOOL_JAR="$HOME/Unity/bundletool-all.jar"
if command -v bundletool &>/dev/null; then
    ok "bundletool found in PATH: $(command -v bundletool)"
elif [[ -f "$BUNDLETOOL_JAR" ]] && command -v java &>/dev/null; then
    ok "bundletool jar found: $BUNDLETOOL_JAR (invoked as: java -jar $BUNDLETOOL_JAR)"
    warn "bundletool is not a standalone command -- use the alias in .bash_aliases.local or add a wrapper script to PATH."
elif [[ -f "$BUNDLETOOL_JAR" ]]; then
    warn "bundletool jar found at $BUNDLETOOL_JAR but java is not on PATH."
else
    warn "bundletool not found. Download from https://github.com/google/bundletool/releases -> save as ~/Unity/bundletool-all.jar"
fi

# ── 2. Install LFS hooks ─────────────────────────────────────────────────────
info ""
info "--- Installing git-lfs hooks ---"
git lfs install
ok "git-lfs hooks installed (globally)"

# ── 3. Install pre-commit hook ───────────────────────────────────────────────
info ""
info "--- Installing pre-commit hook ---"

HOOK_SRC="$REPO_ROOT/Tools/git-hooks/pre-commit"
HOOK_DST="$REPO_ROOT/.git/hooks/pre-commit"

if [[ ! -f "$HOOK_SRC" ]]; then
    fail "Hook source not found: $HOOK_SRC"
fi

# Warn if a pre-commit hook already exists and is NOT our symlink/copy
if [[ -e "$HOOK_DST" && ! -L "$HOOK_DST" ]]; then
    warn "A custom .git/hooks/pre-commit already exists and is not a symlink. Backing it up."
    mv "$HOOK_DST" "${HOOK_DST}.bak.$(date +%s)"
fi

chmod +x "$HOOK_SRC"
ln -sf "$HOOK_SRC" "$HOOK_DST"
ok "pre-commit hook linked: .git/hooks/pre-commit -> Tools/git-hooks/pre-commit"

# ── 4. Copy build.env.example ────────────────────────────────────────────────
info ""
info "--- Checking build.env ---"

ENV_EXAMPLE="$REPO_ROOT/build.env.example"
ENV_TARGET="$HOME/.config/artificial-pi/build.env"

if [[ ! -f "$ENV_EXAMPLE" ]]; then
    warn "build.env.example not found (will be created in Phase 8). Skipping."
else
    if [[ -f "$ENV_TARGET" ]]; then
        warn "$ENV_TARGET already exists. Skipping copy (delete it to reset)."
    else
        mkdir -p "$(dirname "$ENV_TARGET")"
        cp "$ENV_EXAMPLE" "$ENV_TARGET"
        ok "Copied build.env.example -> $ENV_TARGET"
        info "Edit $ENV_TARGET and fill in your keystore credentials before building."
    fi
fi

# ── Done ──────────────────────────────────────────────────────────────────────
info ""
ok "Setup complete. Run 'git commit' to verify the pre-commit hook fires."
