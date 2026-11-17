---
status: in progress
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

- 2026-11-17. **Pre-commit hook**: both bugs seen, then fixed. Before: a force-staged `UnitySetup/Library/LastSceneManagerSetup.txt` and a 2 MB non-LFS `UI Toolkit/big.bin` both passed.
  After: both fail with their own message; a 2 MB `.png` (LFS) and a normal commit pass. Paths are read NUL-separated, the size is the staged blob's (`git cat-file -s`), and `git check-attr -z` replaces an `awk` that also split on spaces.
- **`Tools/gates.sh`**: `compile`, `edit`, `play`, `all`; the project's editor version from `ProjectVersion.txt`.
  Its first run failed: this session has `DISPLAY=:0` set but cannot open it, so "no `DISPLAY`, use Xvfb" was the wrong test. On Linux it now always runs under `xvfb-run` when installed.
  Seen failing on an injected `warning CS0219` (compile gate) and an injected `Assert.Fail` (EditMode gate, naming the test), then green: compile 0 warnings, EditMode 114/114, PlayMode 4/4. The first-import rerun is exercised in phase 05's fresh clone.
- **`Tools/build-android.sh`** took the newest editor under `~/Unity/Hub/Editor/`, not the project's version; it now tries the project's version first and scans only as a fallback.
- **`Tools/setup.sh`**: the adb path hardcoded `6000.4.4f1` (now from `ProjectVersion.txt`); a "will be created in Phase 8" message; and nothing checked the `unityyamlmerge` merge driver that `.gitattributes` names, which is per-user git config (now a warning pointing at `docs/git-workflow.md`).
- **`.gitattributes`**: fonts (`*.ttf`, `*.otf`) added to the LFS rules. The rest is fine: LF everywhere, Unity YAML as text with the merge driver, the usual binaries in LFS.
- Read and fine as they are: `.gitignore` (Unity folders at any depth, secrets, IDE files), `.editorconfig`, `.vscode/settings.json`.
