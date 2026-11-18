# 📄 `docs/systems/logging.md`

## Overview

Single project-wide logging facade and an opt-in on-device debug overlay.

---

## Responsibilities

- Wrap `UnityEngine.Debug` with category tags and structured args
- Strip Info/Verbose calls in Release builds
- Provide an in-build debug overlay for FPS, state, and dev shortcuts

---

## Public interface

```
Log.Verbose(LogCat cat, string message, params object[] args)
Log.Info   (LogCat cat, string message, params object[] args)
Log.Warn   (LogCat cat, string message, params object[] args)
Log.Error  (LogCat cat, string message, params object[] args)
```

`LogCat` is an enum: `Boot`, `Scene`, `Input`, `Save`, `Audio`, `UI`, `Gameplay`, `Build`.

---

## Build-time stripping

- `[Conditional("UNITY_EDITOR")]` and `[Conditional("DEVELOPMENT_BUILD")]` on
  `Verbose` and `Info` so they vanish from Release IL2CPP output.
- `Warn`/`Error` always present.
- The ring of the last 500 entries exists in every build; in Release it holds warnings and errors only.

---

## Diagnostics report

What a person copies or shares off the phone with no adb (`plans/05_diagnostics/`), after abyss's log button: header lines, a blank line, then statistics, problems and log lines. Every build carries it.

- **`DiagnosticsRecorder`** (in `Persistent.unity`) collects, with fixed buffers and no allocation per frame: frame times into a session histogram (0.5 ms bins) and a last-minute ring; memory and managed heap once a second into histograms and a 600-second series; exceptions, errors and asserts from `Application.logMessageReceived` into a `ProblemLog`, apart from the ring, so a busy log never pushes them out (distinct by message and first stack line, with count, first and last time, top 6 stack lines, at most 10).
- **`DiagnosticsReport`** writes it on demand:
  - header: product, version and version code, build kind, commit and time (`SO_BuildInfo`, stamped by `BuildPipeline` around each build and set back to "editor" after), development or release, Unity version; device, OS, CPU, memory; GPU and graphics API; screen, refresh rate, DPI, orientation; quality level, target frame rate, vsync; session time, game state, loaded scenes; battery;
  - statistics for the session and the last 60 s: min, p05, median, mean, p95, max of frame time and frame rate (read off the frame times), memory and heap, and in development builds managed allocation per frame (`gc alloc kB`, the Profiler's "GC Allocated In Frame"); GC collections and frames over 33 ms;
  - problems; the log ring, `seconds level category text`;
  - detailed only: the per-second series (frames, mean and max frame time, memory, heap) of the last 10 minutes.
- Size: a few kB typical (a launch into play logs about 15 lines); the detailed series adds about 40 bytes a second.
- **Opened** from a Diagnostics button on the main menu and the pause screen (`ScreenId.Diagnostics`, `DiagnosticsPanel`): Copy (the report to the clipboard), Copy detailed (with the series), Share (the detailed report to Android's share sheet: save to Files or Drive, send to a chat), Previous session (the last session's kept report to the clipboard), Close.
- **Kept on the phone**: `persistentDataPath/diagnostics/current.txt` every 30 s and on pause and quit; `previous.txt` is the session before, so a crash's report can still be copied after a relaunch.
- **Allocation scan** (development builds): `-allocationScan <file>` on a player's command line plays a new game and measures managed allocation per frame with everything on, then with each component type disabled in turn, without and with a simulated finger on the stick, and writes what each type accounts for. On the box: `Game.Editor.BuildLinuxDevPlayer.BuildFromCommandLine -playerOut <dir>/UnitySetup.x86_64`, then `xvfb-run -a <dir>/UnitySetup.x86_64 -force-vulkan -allocationScan out.txt`. The Editor cannot answer this: its play mode allocates about 150 kB a frame by itself.
- Read one without a phone: `REPORT_OUT=<file>` with the PlayMode `DiagnosticsReportTests`; a picture of the panel: `PANEL_CAPTURE=<file.png>` with `DiagnosticsPanelTests`.

---

## Debug overlay

Toggle: `F1` in Editor, three-finger tap on device.

Displays:

- FPS (avg, 1% low over last 60 frames)
- Frame time (main, render)
- Current `GameManager` state
- The newest 20 log lines

Buttons (dev builds only):

- Reset player position
- Reload current scene
- Skip loading screen

The overlay is a separate `UIDocument` and is **excluded** from Release builds
via a scripting define / asmdef include condition.

---

## Constraints

- Never allocate per call in Release: format only when the level is enabled.
- No file logging in v0.2 - `adb logcat` is the contract.
- No remote crash reporting in v0.2.

---

## Interactions

### Used by

- All systems and gameplay scripts
- Debug overlay (reads recent log buffer)

### Uses

- `UnityEngine.Debug`
- Profiler counters (`Profiler.GetCounterValue`) for the overlay