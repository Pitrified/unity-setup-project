---
status: done
---

# Phase 05 - Per-frame allocations

## Overview

The first phone report showed 8 GC collections in 57 s, the managed heap climbing 9.5 to 14.5 MB between them: something allocates every frame, against the project's rule (no allocations in `Update`). The user, 2026-11-18: "Yes debug that".

## Plan

- Measure before guessing: a PlayMode test boots into play, measures managed bytes allocated per frame on the main thread (`GC.GetAllocatedBytesForCurrentThread`), then disables each component type in the scenes in turn and measures again; the drop is what that type allocates.
- Fix what allocates, worst first; the measurement becomes a test that fails when gameplay allocates per frame again.
- The device confirms: the next report's GC count.

## Done when

- Gates green with the allocation test; a phone report with no (or far fewer) collections.

## What the implementation found

- 2026-11-18. **The first measurement was blind.** `GC.GetAllocatedBytesForCurrentThread` read 0 for a control that allocates 1 KB a frame; the Profiler's "GC Allocated In Frame" saw it (1453 B/frame). `WaveFieldTests.HeightAt_DoesNotAllocate` had used the blind counter since the water phase and proved nothing; `GC.GetTotalMemory` was tried and is blind too (it moves in heap blocks). The check moved to PlayMode on the profiler counter: 1000 `HeightAt` a frame, 406 B/frame, the same as an idle component.
- **The Editor cannot answer it**: in play mode it allocates about 149 kB a frame by itself, and disabling any one component type moved that by ±5 to 12 kB, both ways: noise.
- **A real player can.** `AllocationScan` (development builds, `-allocationScan <file>`) and a Linux development player built on the box (`BuildLinuxDevPlayer`). The first run stalled after boot: a desktop player pauses without focus, so the scan sets `Application.runInBackground`. The second threw re-enabling a component a step had destroyed (a UI rebuild); null-checked and reported.
  Result: **64 B a frame while playing, all of it the uGUI `EventSystem`, and no GC collection in any 3 s step.** No gameplay, water or diagnostics component allocates per frame.
- **Touch, unresolved on the box**: the phone was being steered with the on-screen stick, a known source of uGUI allocation. A simulated finger on a virtual `Touchscreen` never reached the stick (the avatar moved 0.0 m), starting on the knob or off it; with it the numbers did not change, which therefore proves nothing. Left there.
- **What the phone's numbers say instead**: the heap's median (9.5 MB) equals its 5th percentile, so it sat flat most of the session rather than sawtoothing as a per-frame allocation would make it; the 8 collections count from boot, through two scene loads that allocate heavily; the heap was highest at the end, when the panel opened (building the report and laying out its 40-line preview allocates).
- **So the report now measures it on the device**: `gc alloc kB` per frame, from the Profiler's counter, in the statistics (development builds), and "Copy detailed" gives the heap second by second, which shows when it grows.
- The Linux builds rewrote four settings files for desktop (URP prefiltering, Standalone batching, a Connect flag); restored from git. Documented in `BuildLinuxDevPlayer` and `headless-unity.md`, with the other pitfalls of this phase.
- 2026-11-18, **a second phone report** (the same `a330c66` build): the previous session's kept report, after a force quit during play.
  The heap stayed at 9.2 to 9.6 MB for the whole 13.7 s session; the per-second series shows it flat at about 10 MB from 5.7 to 11.7 s of play at a steady 60 fps. All 5 collections fell in the first ~6 s (boot, menu, loading the Game scene), where the only slow frames are too (316 ms at 3.6 s, 100 ms at 5.7 s).
  With the Linux player's 64 B a frame, the conclusion: **gameplay does not allocate per frame; the collections are loads (and, in the first report, opening the panel)**. Nothing to fix in the game.
- Deferred check, done the same day: a report from the new APK (`dev 4e2048e+dirty`, dirty because the plan was being edited during the build). `gc alloc kB` per frame on the phone: **p05, median and p95 0.1 kB**, max 5187 kB (a scene load), mean 12.7 kB (the loads); 7 collections, all from loading; 60 fps held (frame time p05 to p95 16.8 ms). Per-frame allocation in play on the device is about 100 B, as in the Linux player. Closed.
- 2026-11-18, the `dev` APK with the `gc alloc kB` row built from the working tree (git clean after it): **88 MB, against 71 MB before**. Still arm64 only; the largest entries are `libunity.so` (98 MB uncompressed) and `libil2cpp.so` (67 MB); the build log shows no deep profiling or script debugging. Not explained. The guess: the Linux player builds switched the Editor's active platform, and the switch back re-imported everything for Android, possibly changing a setting kept in `Library/` rather than in git. To check next time: build the same commit from a fresh clone and compare sizes.
- 2026-11-18, **the size, explained**: the same commit built from a fresh clone is 71 MB (73.5 MB on disk). The two APKs hold the same entries with the same compressed sizes; the working tree's file had 17.8 MB that belonged to no entry. Gradle packages incrementally, rewriting its previous APK in place (`Library/Bee/Android/Prj/IL2CPP/Gradle/launcher/build/outputs`), and the space of replaced entries stays in the file: several builds in the same tree had piled it up. Not the Linux builds, and nothing installed was larger.
  `Tools/build-android.sh` now removes Gradle's last APK output before building; the next build in the working tree came out at 73.5 MB again. The same change in artificial-pirate's script.
