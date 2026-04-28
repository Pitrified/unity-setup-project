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

---

## Debug overlay

Toggle: `F1` in Editor, three-finger tap on device.

Displays:

- FPS (avg, 1% low over last 60 frames)
- Frame time (main, render)
- Current `GameManager` state
- Last 20 log lines

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