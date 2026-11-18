---
status: done
---

# Phase 03 - Panel

## Plan

- A Diagnostics button on the main menu and the pause screen; a panel (UI Toolkit, through `UISystem`) with Copy, Copy detailed, Share, Previous session, Close, and the report's first lines, a status line ("copied 3.4 kB").
- PlayMode test: the buttons reach the exporter (a fake one).

## Done when

- Gates green; a capture of the panel.

## What the implementation found

- 2026-11-18. `ScreenId.Diagnostics`; `DiagnosticsPanel` (plain class, buttons wired by name, a status line with size and lines, a 40-line preview); `UISystem` builds it from a third document (`DiagnosticsUI`, `Diagnostics.uxml` and `.uss`, sorting order above the pause screen) and the recorder; a Diagnostics button on the pause screen (`UISystem`) and the main menu (`MenuController`, through `GameManager.UISystem`). The panel counts in `IsAnyOverlayOpen`.
- The scene step loaded nothing before opening the scene this time; both references saved.
- `DiagnosticsPanelTests` (PlayMode): boot to the menu, open the panel, press each button with a submit event against a fake exporter: Copy hands over the plain report, Copy detailed and Share the detailed one, Close hides it.
- **A picture of UI Toolkit**: a camera render does not include it (it draws to the screen), and `WaitForEndOfFrame` does not fire in batch mode, so the test gives the panel a copy of its panel settings with a render-texture target for the shot (`PANEL_CAPTURE`). The panel reads well: title, status ("shared 1.5 kB, 37 lines"), the report's start, five buttons at 96 px. The statistics' columns drift a little in the proportional font; a monospace font is a later touch if it bothers.
- Gates: compile 0 warnings, EditMode 133/133, PlayMode 14/14.
