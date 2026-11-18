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
- 2026-11-18, **the first report from the phone**, pasted by the user: Pixel 7 Pro, Android 17 (API 37), Mali-G710 on Vulkan, `dev a330c66`. Copy worked (the paste), Share worked (`shared 4554 characters` in the log).
  - The water scene holds **60 fps**: frame time p05, median and p95 all 16.8 ms over 3030 frames; 8 frames over 33 ms (0.26 %). Memory 147 MB of a 400 MB budget, managed heap 9 to 15 MB.
  - **A measuring artefact**: max frame 3467.6 ms, which also pulled the mean to 55.5 fps. The share sheet sent the app to the background, and the next frame's delta spanned it. The recorder now skips the first frame after the app regains focus or resumes. Gates green after the fix.
  - **A real finding**: 8 GC collections in 57 s, the heap climbing from 9.5 to 14.5 MB between them: something allocates every frame, against the project's rule. Not chased yet; the next step is measuring per-frame allocation by system (`GC.GetAllocatedBytesForCurrentThread` around each `Update` in a PlayMode run, or the Profiler's GC Alloc column at the workstation). The debug overlay, which formats its text ten times a second even while hidden, is the first suspect, not a finding.
  - The display reports 60 Hz on a 120 Hz panel because the game targets 60 fps; consistent.
- Still open, HUMAN: force-quit during play, relaunch, Previous session.
