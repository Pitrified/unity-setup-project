---
status: in progress
---

# Phase 04 - Verify

## Plan

- Fresh clone on disk: gates, `dev` APK.
- HUMAN on the phone: Copy and paste here; Share to Files or a chat; force-quit, relaunch, Previous session.

## Done when

- The box checks pass and a report from the phone has been pasted here.

## What the implementation found

- 2026-11-18, fresh clone of `feat/05_diagnostics` at `a330c66` under `~/.cache/`: `plans.py check`, link check (the 8 old diary links), `Tools/gates.sh all` from an empty `Library/`: compile 0 warnings, EditMode 133/133, PlayMode 14/14.
- `dev` APK 71 MB, `com.pitrified.unitysetup`. The stamp worked: `a330c66` is in the APK's `sharedassets1.assets`, and after the build `SO_BuildInfo.asset` is back to "editor" with git status clean. Copied to the working repo's `UnitySetup/Build/dev/`.
- Open, HUMAN on the phone: Diagnostics from the menu and from pause; Copy and paste a report here; Share to Files or a chat; force-quit during play, relaunch, Previous session.
