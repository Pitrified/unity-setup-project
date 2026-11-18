---
status: planned
---

# Phase 02 - Export

## Plan

- `IDiagnosticsExport` with `Copy(text)` (`GUIUtility.systemCopyBuffer`) and `Share(text)` (Android: `Intent.ACTION_SEND`, text/plain, through `AndroidJavaObject`, wrapped in a chooser; elsewhere: write a file and log its path).
- The kept file: the report written to `persistentDataPath/diagnostics/current.txt` every 30 s, on pause and on quit; at start the last one moves to `previous.txt`, which the panel can copy or share.
- Tests: the file rotation (PlayMode, a temporary folder), the exporter chosen per platform.

## Done when

- Gates green; on the box, the files appear and rotate.

## What the implementation found
