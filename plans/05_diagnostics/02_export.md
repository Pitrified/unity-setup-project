---
status: done
---

# Phase 02 - Export

## Plan

- `IDiagnosticsExport` with `Copy(text)` (`GUIUtility.systemCopyBuffer`) and `Share(text)` (Android: `Intent.ACTION_SEND`, text/plain, through `AndroidJavaObject`, wrapped in a chooser; elsewhere: write a file and log its path).
- The kept file: the report written to `persistentDataPath/diagnostics/current.txt` every 30 s, on pause and on quit; at start the last one moves to `previous.txt`, which the panel can copy or share.
- Tests: the file rotation (PlayMode, a temporary folder), the exporter chosen per platform.

## Done when

- Gates green; on the box, the files appear and rotate.

## What the implementation found

- 2026-11-18. `IDiagnosticsExport` and `PlatformDiagnosticsExport`: Copy through `GUIUtility.systemCopyBuffer`; Share on Android through `Intent.ACTION_SEND` (text/plain, subject and text, in a chooser) with `AndroidJavaObject`, no plugin or permission; elsewhere Share writes `diagnostics/shared.txt` and logs the path. `DiagnosticsFiles`: `current.txt` written atomically (temporary file, then replace), `previous.txt` from the last session at start.
- The recorder rotates at start, writes the plain report every 30 s and the detailed one on pause and on quit, and gives the panel `PreviousReport` and `SaveNow`. File errors are warnings in the log, never exceptions in the game.
- On the box after two PlayMode runs: `previous.txt` 2.6 kB (the earlier run), `current.txt` 7.7 kB (detailed, written when play mode ended), `shared.txt` from the share test.
- The Android share path cannot run on the box; it is the phone's check in phase 04. A report is far under the intent size limit (about 1 MB of binder transaction).
- Tests (EditMode): rotation keeps the last write of the last session and a later rotation replaces it; share without a share sheet writes the file. Gates: compile 0 warnings, EditMode 133/133, PlayMode 13/13.
