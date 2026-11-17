---
status: in progress
---

# Phase 05 - Verify

## Plan

- A fresh clone of the branch on disk (not `/tmp`, a RAM tmpfs with a quota on this box): `Tools/setup.sh`, `Tools/gates.sh all`, `Tools/build-android.sh dev`, link check, `plans.py check`.
- HUMAN: the `dev` APK on the phone: it installs as "Unity Setup" next to Artificial Pirate, boots to the menu, the capsule moves, pause and resume, and the position is restored after a restart.

## Done when

- The box checks pass in the fresh clone, and the device run is in the log.

## What the implementation found

- 2026-11-18, fresh clone of `feat/02_guide_rebuild` at `b4f16b5` under `~/.cache/` (disk), all box checks pass:
  `Tools/setup.sh` exit 0 (merge driver found configured; `bundletool` missing, AAB only); `git lfs ls-files` empty; `plans.py check`; link check with only the 8 old diary links;
  `Tools/gates.sh all` from an empty `Library/`: the first-import rerun fired as designed, then compile 0 warnings, EditMode 114/114, PlayMode 4/4;
  `dev` APK 70 MB, `com.pitrified.unitysetup`, `Unity Setup`, `arm64-v8a`. The APK is copied to `UnitySetup/Build/dev/` in the working repo for the phone.
- Open: the HUMAN device run.
