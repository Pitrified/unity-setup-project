#!/usr/bin/env bash
# Tools/gates.sh
# Runs the gates of docs/headless-unity.md §3 in Unity batch mode:
# compile with 0 errors and 0 warnings, EditMode and PlayMode tests.
#
# Usage:
#   ./Tools/gates.sh [compile|edit|play|all]     (default: all)
#
# The editor is the project's version (ProjectSettings/ProjectVersion.txt) under
# ~/Unity/Hub/Editor/; UNITY_PATH overrides it. On Linux Unity runs under xvfb-run when it
# is installed. Logs and test results go to <project>/Logs/gates/.
# Exit code: 0 when every gate asked for passes, 1 otherwise.
# Close the Editor on this project first: two Unity processes cannot open one project.

set -euo pipefail

RED='\033[0;31m'
GREEN='\033[0;32m'
NC='\033[0m'

info() { echo "[gates]        $*"; }
ok()   { echo -e "${GREEN}[gates]  PASS  $*${NC}"; }
bad()  { echo -e "${RED}[gates]  FAIL  $*${NC}" >&2; }
die()  { bad "$*"; exit 1; }

WHAT="${1:-all}"
case "$WHAT" in
    compile|edit|play|all) ;;
    *) die "Unknown gate '$WHAT'. Usage: $0 [compile|edit|play|all]" ;;
esac

# ── Paths ─────────────────────────────────────────────────────────────────────
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT_DIR="$REPO_ROOT/UnitySetup"
VERSION_FILE="$PROJECT_DIR/ProjectSettings/ProjectVersion.txt"
OUT_DIR="$PROJECT_DIR/Logs/gates"

[[ -f "$VERSION_FILE" ]] || die "Unity project not found: $VERSION_FILE is missing."
VERSION="$(sed -n 's/^m_EditorVersion: //p' "$VERSION_FILE")"

if [[ -n "${UNITY_PATH:-}" ]]; then
    UNITY_BIN="$UNITY_PATH"
elif [[ "$(uname)" == "Darwin" ]]; then
    UNITY_BIN="/Applications/Unity/Hub/Editor/$VERSION/Unity.app/Contents/MacOS/Unity"
else
    UNITY_BIN="$HOME/Unity/Hub/Editor/$VERSION/Editor/Unity"
fi
[[ -x "$UNITY_BIN" ]] || die "Unity $VERSION not found at $UNITY_BIN. Install that version or set UNITY_PATH."

mkdir -p "$OUT_DIR"
info "Unity   : $UNITY_BIN"
info "Project : $PROJECT_DIR"
info "Output  : $OUT_DIR"

# ── Running Unity ─────────────────────────────────────────────────────────────
# Vulkan on Linux renders on the GPU; without it, under Xvfb, Unity falls back to the CPU.
# No -nographics: PlayMode tests and captures render.
UNITY_ARGS=(-batchmode -projectPath "$PROJECT_DIR")
[[ "$(uname)" == "Linux" ]] && UNITY_ARGS+=(-force-vulkan)

# On Linux, Unity always runs under its own virtual display when xvfb-run is installed:
# a session over SSH can have DISPLAY set to a desktop it cannot open.
PREFIX=()
if [[ "$(uname)" == "Linux" ]]; then
    if command -v xvfb-run >/dev/null; then
        PREFIX=(xvfb-run -a -s "-screen 0 1280x1024x24")
    elif [[ -z "${DISPLAY:-}" ]]; then
        die "No X display and xvfb-run not installed (apt install xvfb)."
    fi
fi

# run_unity <log name> <args...>: runs Unity, returns its exit code.
run_unity() {
    local name="$1"; shift
    local log="$OUT_DIR/$name.log"
    info "Running $name (log: $log)"
    set +e
    "${PREFIX[@]}" "$UNITY_BIN" "${UNITY_ARGS[@]}" -logFile "$log" "$@"
    local code=$?
    set -e
    return $code
}

# compiler_lines <log>: the error CS / warning CS lines, one each.
compiler_lines() {
    grep -E "error CS|warning CS" "$1" | sort -u || true
}

# ── Gates ─────────────────────────────────────────────────────────────────────
gate_compile() {
    local first_import=0
    [[ -d "$PROJECT_DIR/Library" ]] || first_import=1

    local code=0
    run_unity compile -quit || code=$?
    local lines
    lines="$(compiler_lines "$OUT_DIR/compile.log")"

    # A first import from an empty Library/ logs Shader Graph errors from the package
    # cache once (a known Unity 6 import-order issue); a second open is clean.
    if [[ $first_import -eq 1 && -n "$lines" ]] \
        && ! grep -qv "^Library/PackageCache/" <<<"$lines"; then
        info "First import from an empty Library/: package cache errors, opening once more."
        code=0
        run_unity compile -quit || code=$?
        lines="$(compiler_lines "$OUT_DIR/compile.log")"
    fi

    if [[ $code -ne 0 || -n "$lines" ]]; then
        [[ -n "$lines" ]] && echo "$lines" >&2
        bad "compile: Unity exit $code, $(grep -c . <<<"$lines" || true) error/warning line(s)"
        return 1
    fi
    ok "compile: 0 errors, 0 warnings"
}

# gate_tests <EditMode|PlayMode>
gate_tests() {
    local platform="$1"
    local xml="$OUT_DIR/$platform.xml"
    rm -f "$xml"

    local code=0
    run_unity "$platform" -runTests -testPlatform "$platform" -testResults "$xml" || code=$?

    local lines
    lines="$(compiler_lines "$OUT_DIR/$platform.log")"
    [[ -f "$xml" ]] || { [[ -n "$lines" ]] && echo "$lines" >&2; bad "$platform: no results file (Unity exit $code)"; return 1; }

    # The root <test-run> element carries the totals.
    local root total failed
    root="$(grep -m1 -o '<test-run [^>]*' "$xml")"
    total="$(grep -o ' total="[0-9]*"' <<<"$root" | grep -o '[0-9]*')"
    failed="$(grep -o ' failed="[0-9]*"' <<<"$root" | grep -o '[0-9]*')"

    if [[ $code -ne 0 || "${failed:-1}" != "0" || -n "$lines" ]]; then
        grep -o '<test-case [^>]*result="Failed"[^>]*' "$xml" \
            | grep -o 'fullname="[^"]*"' | sed 's/^/  /' >&2 || true
        [[ -n "$lines" ]] && echo "$lines" >&2
        bad "$platform: $failed of $total failed (Unity exit $code; failure messages in $xml)"
        return 1
    fi
    ok "$platform: $total of $total passed"
}

status=0
case "$WHAT" in
    compile) gate_compile || status=1 ;;
    edit)    gate_tests EditMode || status=1 ;;
    play)    gate_tests PlayMode || status=1 ;;
    all)
        gate_compile || status=1
        if [[ $status -eq 0 ]]; then
            gate_tests EditMode || status=1
            gate_tests PlayMode || status=1
        fi
        ;;
esac

[[ $status -eq 0 ]] && ok "gates: $WHAT" || bad "gates: $WHAT"
exit $status
