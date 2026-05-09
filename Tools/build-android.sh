#!/usr/bin/env bash
# Tools/build-android.sh
# Builds an Android APK or AAB for the artificial-pi Unity project.
#
# Usage:
#   ./Tools/build-android.sh dev         → development APK (fast, debug keystore)
#   ./Tools/build-android.sh profile     → profiling APK   (IL2CPP, deep profiling)
#   ./Tools/build-android.sh release     → release AAB     (signed, stripped, LZ4HC)
#
# Environment variables (set in ~/.config/artificial-pi/build.env):
#   UNITY_PATH              path to the Unity Editor binary (optional; auto-detected from Hub)
#   ANDROID_KEYSTORE_PATH   absolute path to upload keystore (release only)
#   ANDROID_KEYSTORE_PASS   keystore password                (release only)
#   ANDROID_KEY_ALIAS       key alias                        (release only)
#   ANDROID_KEY_PASS        key alias password               (release only)
#
# See docs/build-and-release.md for full context.

set -euo pipefail

# ── Colours ──────────────────────────────────────────────────────────────────
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
RED='\033[0;31m'
NC='\033[0m'

ok()   { echo -e "${GREEN}[build]  OK    $*${NC}"; }
warn() { echo -e "${YELLOW}[build]  WARN  $*${NC}"; }
fail() { echo -e "${RED}[build]  FAIL  $*${NC}" >&2; exit 1; }
info() { echo -e "[build]        $*"; }

# ── Argument parsing ──────────────────────────────────────────────────────────
MODE="${1:-}"
if [[ -z "$MODE" || ! "$MODE" =~ ^(dev|profile|release)$ ]]; then
    echo "Usage: $(basename "$0") <dev|profile|release>"
    echo ""
    echo "  dev      → development APK (debug keystore, no stripping)"
    echo "  profile  → profiling APK   (IL2CPP, deep profiling, debug keystore)"
    echo "  release  → release AAB     (upload keystore, stripped, auto-bump versionCode)"
    exit 1
fi

# ── Resolve paths ─────────────────────────────────────────────────────────────
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT_DIR="$REPO_ROOT/UnitySetupPrpj"

if [[ ! -d "$PROJECT_DIR/Assets" ]]; then
    fail "Unity project not found at $PROJECT_DIR (expected Assets/ subfolder)."
fi

# ── Source build.env ──────────────────────────────────────────────────────────
BUILD_ENV_FILE="$HOME/.config/artificial-pi/build.env"
if [[ -f "$BUILD_ENV_FILE" ]]; then
    # shellcheck source=/dev/null
    set +u
    source "$BUILD_ENV_FILE"
    set -u
    info "Loaded env from $BUILD_ENV_FILE"
fi

# ── Validate release-only env vars ────────────────────────────────────────────
if [[ "$MODE" == "release" ]]; then
    MISSING=""
    for var in ANDROID_KEYSTORE_PATH ANDROID_KEYSTORE_PASS ANDROID_KEY_ALIAS ANDROID_KEY_PASS; do
        if [[ -z "${!var:-}" ]]; then
            MISSING="$MISSING  $var\n"
        fi
    done
    if [[ -n "$MISSING" ]]; then
        fail "Missing required env vars for release build. Set them in $BUILD_ENV_FILE:\n${MISSING}"
    fi
    if [[ ! -f "${ANDROID_KEYSTORE_PATH}" ]]; then
        fail "Keystore not found at '${ANDROID_KEYSTORE_PATH}' (ANDROID_KEYSTORE_PATH)."
    fi
    ok "Keystore validated: ${ANDROID_KEYSTORE_PATH}"
fi

# ── Resolve Unity binary ──────────────────────────────────────────────────────
UNITY_BIN="${UNITY_PATH:-}"

if [[ -z "$UNITY_BIN" ]]; then
    # Scan Unity Hub default install paths (Linux then macOS).
    while IFS= read -r -d '' candidate; do
        if [[ -x "$candidate" ]]; then
            UNITY_BIN="$candidate"
            break
        fi
    done < <(find \
        "$HOME/Unity/Hub/Editor" \
        /Applications/Unity/Hub/Editor \
        -name "Unity" -o -name "Unity.app" \
        2>/dev/null \
        | sort -rV \
        | while IFS= read -r p; do
            # macOS .app → Contents/MacOS/Unity
            if [[ "$p" == *.app ]]; then
                echo -n "$p/Contents/MacOS/Unity"$'\0'
            else
                echo -n "$p"$'\0'
            fi
          done)
fi

if [[ -z "$UNITY_BIN" || ! -x "$UNITY_BIN" ]]; then
    fail "Unity binary not found. Set UNITY_PATH=/path/to/Unity in $BUILD_ENV_FILE \nor install Unity via Unity Hub."
fi
ok "Unity binary: $UNITY_BIN"

# ── Determine executeMethod and artifact ──────────────────────────────────────
case "$MODE" in
    dev)
        METHOD="Game.Editor.BuildPipeline.BuildDev"
        ARTIFACT="$PROJECT_DIR/Build/dev/${PRODUCT_NAME:-artificial-pi}-dev.apk"
        ;;
    profile)
        METHOD="Game.Editor.BuildPipeline.BuildProfile"
        ARTIFACT="$PROJECT_DIR/Build/profile/${PRODUCT_NAME:-artificial-pi}-profile.apk"
        ;;
    release)
        METHOD="Game.Editor.BuildPipeline.BuildRelease"
        ARTIFACT="$PROJECT_DIR/Build/release/${PRODUCT_NAME:-artificial-pi}-release.aab"
        ;;
esac

# ── Prepare log path ─────────────────────────────────────────────────────────
BUILD_LOG="$PROJECT_DIR/Build/build-${MODE}.log"
mkdir -p "$(dirname "$BUILD_LOG")"

# ── Print summary ─────────────────────────────────────────────────────────────
info ""
info "Mode    : $MODE"
info "Project : $PROJECT_DIR"
info "Method  : $METHOD"
info "Log     : $BUILD_LOG"
info "Artifact: $ARTIFACT"
info ""

# ── Invoke Unity in batchmode ─────────────────────────────────────────────────
info "Starting Unity build…"
UNITY_EXIT=0
"$UNITY_BIN" \
    -batchmode \
    -nographics \
    -quit \
    -projectPath "$PROJECT_DIR" \
    -buildTarget Android \
    -executeMethod "$METHOD" \
    -logFile "$BUILD_LOG" \
    || UNITY_EXIT=$?

# ── Check exit code ──────────────────────────────────────────────────────────
if [[ $UNITY_EXIT -ne 0 ]]; then
    echo ""
    warn "Unity exited with code $UNITY_EXIT. Last 40 lines of build log:"
    echo "────────────────────────────────────────"
    tail -n 40 "$BUILD_LOG" 2>/dev/null || echo "(log not found)"
    echo "────────────────────────────────────────"
    fail "Build FAILED (exit code $UNITY_EXIT). Full log: $BUILD_LOG"
fi

# Scan log for Unity error markers even when exit code is 0
# (Unity occasionally exits 0 but prints error lines).
if grep -qE "^Error |Aborting batchmode due to failure|^\[BuildPipeline\] Build FAILED" \
        "$BUILD_LOG" 2>/dev/null; then
    echo ""
    warn "Error lines detected in build log:"
    grep -E "^Error |Aborting batchmode|Build FAILED" "$BUILD_LOG" | tail -n 10
    fail "Build FAILED (errors in log). Full log: $BUILD_LOG"
fi

# ── Verify artifact was produced ─────────────────────────────────────────────
if [[ ! -f "$ARTIFACT" ]]; then
    fail "Unity exited 0 but artifact not found at $ARTIFACT"
fi

# ── Stage version code bump for release ──────────────────────────────────────
if [[ "$MODE" == "release" ]]; then
    SETTINGS_FILE="$PROJECT_DIR/ProjectSettings/ProjectSettings.asset"
    if git -C "$REPO_ROOT" diff --name-only HEAD | grep -q "ProjectSettings/ProjectSettings.asset"; then
        git -C "$REPO_ROOT" add "$SETTINGS_FILE"
        ok "Staged ProjectSettings/ProjectSettings.asset (bumped versionCode)"
    else
        warn "ProjectSettings.asset not modified; versionCode may not have changed."
    fi
fi

# ── Print artifact info ───────────────────────────────────────────────────────
echo ""
ok "Build SUCCEEDED"
info "Artifact : $ARTIFACT"
info "Size     : $(du -sh "$ARTIFACT" | cut -f1)"
if command -v sha256sum &>/dev/null; then
    info "SHA256   : $(sha256sum "$ARTIFACT" | cut -d' ' -f1)"
elif command -v shasum &>/dev/null; then
    info "SHA256   : $(shasum -a 256 "$ARTIFACT" | cut -d' ' -f1)"
fi
echo ""
