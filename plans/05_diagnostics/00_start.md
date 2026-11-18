---
status: draft
priority: 0
description: |
  An in-app diagnostics report a person can copy or share from the phone, with no adb and no
  connection to the box: a header (app, version, build, device, settings, state) and timestamped
  lines (events, warnings, a periodic frame-rate and memory summary), after abyss's log button.
---

# Diagnostics report

Raised 2026-11-18 by the user, after the water APK: "add a log dump section in the app, so that we can download a file or copy to clipboard some data. most of the time we can assume adb debug is off and i'm not connected to the g7 machine. so collecting data and being able to export and paste here is probably useful. check abyss for the pattern (header with app version and context, then lines with info) we can adapt it here as needed. assess".

## The pattern in abyss

`ui/phone_web/js/log_button.js` and `state.js` (read 2026-11-18 at `235fe95`):

- **A ring of lines**, the newest 500, each `seconds-since-start text`: events as they happen (version, page, config, camera, files fetched, errors) and **a summary once a second** from the main loop (what it is doing, fps, timings).
- **A button** copies a **header** (app and version, URL, user agent, the current config, display, timing median and fps, status and error) then a blank line, then the lines, to the clipboard. If the clipboard is refused, the text appears selected in a text box for a manual copy.
- An end-to-end test checks the button puts the header and lines on the clipboard.

## What is here now

- `Game.Systems.Log`: `Verbose` and `Info` compile only in the Editor and Development builds; `Warn` and `Error` always. A ring of **20** entries.
- `DebugOverlayController` (in `Persistent.unity`): FPS average and 1% low, CPU and GPU frame time, game state, the last 20 log lines, three debug buttons; **Editor and Development builds only**, opened with F1 or a **three-finger tap**.
- The `dev` APK is a Development build (`BuildPipeline`, `BuildOptions.Development`); `release` is not. Version `0.1.0`, version code 1; nothing records the commit a build came from.
- Nothing can leave the phone without adb.

## Proposal

- **One report, two parts, as abyss**:
  - header: app and version (`Application.version`, version code), build (commit and time, stamped by `BuildPipeline`), development or release, device model, OS, CPU, memory, GPU and graphics API, screen size, DPI and refresh rate, quality level and target frame rate, time since start, game state and scene;
  - a blank line, then the lines: `seconds text`, the newest 500: every log entry the build compiles (Info and up in Development, Warn and Error in Release), and a **summary every 5 s**: fps average and 1% low, frame time, memory in use, game state; plus what a system chooses to add (the water: floaters and ripple impulses in the last 5 s).
- **Export without adb**:
  - **Copy**: to the clipboard (`GUIUtility.systemCopyBuffer`, works on Android).
  - **Share**: Android's share sheet with the report as text (`Intent.ACTION_SEND` through `AndroidJavaObject`, no plugin, no permission): save it to Files or Drive, or send it to a chat.
  - **Kept on the phone**: the report written to `persistentDataPath` every 30 s and on pause, and the previous session's file kept, so after a crash the next launch can still copy or share what happened before it.
- **Where**: a Diagnostics button on the main menu and the pause screen, opening a small panel with Copy, Share, Previous session and the last lines. The overlay stays as it is and shows the same ring.
- **Tests**: the report's format (EditMode), the ring and the periodic summary (PlayMode), the panel's buttons call the exporter (PlayMode, with the Android calls behind an interface). HUMAN: Copy and Share on the phone, and a paste here.

## Open questions

- Q1: which builds carry it?
  - a. all builds: Release testers are the ones without adb; Release lines are warnings, errors and the summaries, no Info.
  - b. Development builds only, like the overlay.
  - Recommended: a. It costs a ring of strings and a summary every 5 s; the person who most needs it is a tester on a Play Store build.
- Q2: where is it opened?
  - a. a Diagnostics button on the main menu and the pause screen.
  - b. inside the debug overlay (three-finger tap).
  - c. both.
  - Recommended: a. A three-finger tap is undiscoverable and the overlay is Development-only; a button in two screens a person already visits needs no explanation.
- Q3: export routes.
  - a. Copy, Share and the kept file with the previous session (above).
  - b. Copy only, as abyss.
  - Recommended: a. Copy covers pasting here; Share covers a long report or a crash; the kept file covers "it crashed, now what".
- Q4: stamp the commit and build time into the app?
  - Recommended: yes. `BuildPipeline` writes them into a settings asset the report reads (no `Resources.Load`); a report that does not say which build it came from is half a report.
- Q5: the summary period and ring size.
  - Recommended: every 5 s, 500 lines: about 40 minutes of summaries, or less when events are busy; a report of 500 lines is about 40 kB, fine for the clipboard and a chat.
- Q6: artificial-pirate.
  - Recommended: built here first; that repo copies it when it wants, like the water.

## Phases, once answered

1. **Report and ring**: `DiagnosticsReport` (header and lines), a 500-line ring in `Log`, the periodic summary, the build stamp; EditMode tests.
2. **Export**: clipboard, Android share intent, the kept file and the previous session; behind an interface for tests.
3. **Panel**: the Diagnostics button and panel on the menu and pause screens.
4. **Verify**: gates, APK; HUMAN on the phone: Copy, Share, Previous session after a forced quit, a paste here.

## Out of scope

- Crash reporting services, analytics, anything that sends data off the phone by itself (the spec rules them out).
- Native crash stacks (a managed exception's message and stack are in the lines; a native crash ends the session and the kept file has what came before).
