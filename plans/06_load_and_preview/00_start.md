---
status: in progress
priority: 0
description: |
  Two leftovers of the diagnostics work: find what allocates about 5 MB of managed memory on each
  scene load (and fix it if it is the game's code), and show the diagnostics preview in a
  monospace font so the statistics' columns line up.
---

# Scene-load allocations and the monospace preview

Asked 2026-11-18 by the user, quoting the two optional items left after `05_diagnostics` (the load allocations, which the agent had recommended leaving, and the monospace preview), before porting the guide's systems into artificial-pirate.

## What is known

From [`05_diagnostics/05_allocations.md`](../05_diagnostics/05_allocations.md):

- On the phone, `gc alloc kB` per frame is 0.1 at p05, median and p95; the max, 5187 kB, is a frame that loads a scene. All GC collections fall in loading.
- In a Linux development player, play allocates 64 B a frame (the uGUI `EventSystem`).
- Loads happen behind the loading overlay, so a collection there is not seen by the player. The question is whether the 5 MB is Unity's own (scene deserialisation, UI Toolkit building its panels, shader variants) or game code doing work in `Awake` / `OnEnable` that could be cheaper.

From [`05_diagnostics/03_panel.md`](../05_diagnostics/03_panel.md): the statistics' columns drift in the proportional default font.

## Decisions

- **Measure in a development player with the Profiler log, then read it in the Editor.** The Editor's own play mode allocates about 150 kB a frame, too noisy to attribute; the Linux development player (`BuildLinuxDevPlayer`) is clean. The player writes a Profiler capture (`-profiler-enable -profiler-log-file`), and an Editor tool reads it in batch mode (`ProfilerDriver.LoadProfile`, `RawFrameDataView`) and sums `GC.Alloc` samples under their parent markers for the frames that load a scene. That names the code that allocates, which disabling components cannot do for a load.
- **Fix only what is the game's own and cheap to fix.** Unity's deserialisation and UI Toolkit setup stay; they are recorded in the docs as the expected cost of a load.
- **The font: Liberation Mono Regular** (SIL Open Font License 1.1, 320 kB), shipped with its licence text, as a UI Toolkit font asset used only by the preview label. A desktop-only font on the box is not in the build, so the file is copied into the project.

## Phases

See [`tracking.md`](tracking.md).
