---
status: planned
---

# Phase 03 - Template review

## Overview

The repo scaffold, read as what a new project copies.

## Plan

- Pre-commit hook (Q9): the generated-folder check matches the folders at any depth (`(^|/)(Library|Temp|Logs|Build|Builds|obj)/`); the staged-file loops read NUL-separated names, so `UI Toolkit/` is one path. Seen failing on a staged `UnitySetup/Library/` file before the fix, passing after, and passing on a normal commit.
- `Tools/gates.sh [compile|edit|play|all]` (Q8): the commands of `docs/headless-unity.md` §3, the editor found as `build-android.sh` finds it, logs and test XML in `UnitySetup/Logs/gates/` (ignored), exit non-zero on `error CS`, `warning CS`, a failed test or a missing result file; the Shader Graph errors of a first import from an empty `Library/` handled (a second compile, said in the output). Seen failing on an injected warning and a failing test, then passing.
- Read `Tools/setup.sh`, `.gitattributes`, `.gitignore`, `.editorconfig`, `.vscode/settings.json` against a new project; fix what is wrong, list what is fine.
- Same hook fix and `gates.sh` in artificial-pirate, as one change there on its own branch.

## Done when

- The hook and `gates.sh` seen failing and passing here, and `gates.sh all` green in artificial-pirate.

## What the implementation found
