---
status: done
---

# Phase 01 - Scene-load allocations

## Plan

- Build the Linux development player (`BuildLinuxDevPlayer`) and run it with the allocation scan, which boots, loads the Game scene and plays, with the Profiler logging to a file.
- An Editor tool, batch mode: load the capture, find the frames with the most "GC Allocated In Frame", and sum their `GC.Alloc` samples by the marker above them; print the top markers per frame.
- Game code in the top markers: make it allocate less if that is cheap (reuse, lazy work, nothing built twice). Unity's own: record it in `docs/systems/logging.md` as the expected cost of a load.
- Restore the settings files the Linux build rewrites.

## Done when

- The 5 MB is attributed, by marker, in this file; any game-code fix measured again; gates green.

## What the implementation found

- 2026-11-18. `AllocationScan` now has the Profiler log the boot and the Game scene's load to `<file>.profile.raw`; `Game.Editor.ProfileCaptureReport` reads it in batch mode (`ProfilerDriver.LoadProfile`, `HierarchyFrameDataView` with the GC column) and writes the markers that allocate directly over the whole capture, then the call tree of the six heaviest frames.
- **The first capture was polluted by the desktop renderer**: the Linux player starts on the PC quality level, whose renderer has SSAO; with shaders stripped for Android, URP logged "Couldn't find the required resources for the ScreenSpaceAmbientOcclusion render feature" with a stack trace every frame (11,919 times in one run). That made 850 kB of the capture's stack traces, and it is why the scan ran past its 5-minute timeout. Not on the phone (its reports carry no such problem). The scan now switches to the Mobile quality level first; the next run logged it 0 times and finished in about 5 minutes.
- **The clean capture**: boot, menu and the Game scene's load allocate 4.8 MB on the main thread. Largest frames: 1.7 MB (the menu's first UI Toolkit layout and repaint), 1.6 MB (URP's first frame), 1.2 MB (the menu scene's `UIDocument.OnEnable` and its panels), 0.2 MB (the Game scene: the Input System's `OnScreenControl.OnEnable`).
  By marker: URP's first render 1265 kB, `FontAsset.UpdateGlyphAdjustmentRecords` 637 kB and `AddSynthesizedCharacters` 232 kB (a font read on first use), Mono's JIT 530 kB (not in IL2CPP), UI Toolkit layout measure 464 kB, `UIDocument.OnEnable` 370 kB, render data 321 kB, style 114 kB, on-screen controls 104 kB. The game's own code: `GameBootstrap.Start` 17 kB.
- **Conclusion**: the load cost is Unity's one-off setup, behind the loading overlay; nothing of the game's to fix. Recorded in `docs/systems/logging.md`; the SSAO pitfall in `docs/headless-unity.md`.
- Not explained from the box: the phone's single-frame maximum, 5187 kB, against 1.7 MB here. The phone is IL2CPP with a different screen and GPU, and its frames are counted by the same Profiler counter; a phone capture (the `profile` APK with the Profiler attached) would settle it if it ever matters.
- Scan after the change: 64 B a frame playing, 0 collections, as before. The Linux build's rewritten settings restored from git. Gates: compile 0 warnings, EditMode 132/132, PlayMode 16/16.
