---
status: done
---

# Phase 02 - Monospace preview

## Plan

- Copy Liberation Mono Regular and its licence (SIL OFL 1.1) into `UnitySetup/Assets/UI/Fonts/`; make a dynamic UI Toolkit font asset from it.
- `Diagnostics.uss`: the preview label uses it (`-unity-font-definition`); nothing else changes font.
- The panel capture from `DiagnosticsPanelTests` (`PANEL_CAPTURE`) shows the statistics' columns aligned.

## Done when

- Gates green; a capture with aligned columns; the APK size change noted.

## What the implementation found

- 2026-11-18. Liberation Mono Regular 2.1.5 copied from the box's `fonts-liberation` package into `UnitySetup/Assets/UI/Fonts/`, with `LiberationMono-OFL.txt` (copyright lines and the licence, from the package's copyright file). No font asset was needed: `-unity-font-definition` takes the imported font directly, and UI Toolkit makes a dynamic atlas from it.
- `.diag-preview`: the font, and `white-space: pre-wrap` so runs of spaces stay as written (long lines still wrap).
- The panel capture (`PANEL_CAPTURE`): the header, both statistics blocks and the log in monospace; min, p05, p50, mean, p95 and max line up in their columns.
- APK: 73,658,414 bytes, 186 kB more than before. Git clean after the build.
- Gates: compile 0 warnings, EditMode 132/132, PlayMode 16/16.
- Seen in the capture, not from this change: the Editor's run logs "Quality level 'Mobile' not found" at boot, because the Editor's platform there is desktop. The phone's reports say `quality Mobile`.
