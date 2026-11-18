---
status: done
---

# Phase 04 - Verify

## Plan

- A fresh clone on disk: `uv sync` in `Tools/asset_builders/`, every builder regenerates its FBX, `Tools/gates.sh all`, `Tools/build-android.sh dev`, link check, `plans.py check`.
- HUMAN, later: the barrels and the crate on the phone.

## Done when

- The box checks pass in the fresh clone.

## What the implementation found

- 2026-11-18, fresh clone of `feat/03_asset_method` at `bab2b2b` under `~/.cache/` (disk): LFS brought the two FBX and two reference photographs; `uv sync` and both builders regenerated their FBX byte-identical (git status clean); `plans.py check`; link check with the 8 old diary links only;
  `Tools/gates.sh all` from an empty `Library/`: compile 0 warnings, EditMode 122/122, PlayMode 4/4; `dev` APK 70 MB, `com.pitrified.unitysetup`. The APK is copied to the working repo's `UnitySetup/Build/dev/` for the phone.
- Open, HUMAN: the barrels and crates on the phone.
