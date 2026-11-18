# Diagnostics - implementation tracking

An in-app report a person copies or shares from the phone without adb: header, statistics, exceptions, the last log lines, a detailed series on request. Brainstorm, measurements and answers in [`00_start.md`](00_start.md).

## Key decisions

From `00_start.md`, 2026-11-18:

- **All builds** (Q1); Release keeps warnings, errors, statistics and exceptions, no Info lines.
- **A Diagnostics button on the main menu and the pause screen** (Q2).
- **Copy, Share, and a kept file with the previous session** (Q3).
- **The build stamps its commit and time** (Q4).
- **Statistics, not periodic lines** (Q5, the user's design): histograms for the session, a last-minute window, exceptions kept apart, 500 log lines, a per-second series only on request.

## Phases

| #  | Phase                       | Plan                                   | Status  |
| -- | --------------------------- | -------------------------------------- | ------- |
| 01 | Recorder and report         | [`01_report.md`](01_report.md)         | done    |
| 02 | Export                      | [`02_export.md`](02_export.md)         | done    |
| 03 | Panel                       | [`03_panel.md`](03_panel.md)           | done    |
| 04 | Verify                      | [`04_verify.md`](04_verify.md)         | planned |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-18 : raised by the user after the water APK; abyss's log button read; assessment and Q1 to Q6 written; the user revised Q5 (statistics, exceptions apart, detail on request) after a measurement of how much the game logs; all answered. Four phases.
- 2026-11-18 : phase 01 - recorder, report, build stamp, 500-line ring in every build; quality-name bug fixed; the lost-reference pitfall's real cause (OpenScene unloads assets) found. A real report is 1.9 kB. Gates green.
- 2026-11-18 : phase 02 - copy, Android share intent, kept current and previous reports; files seen rotating on the box. Gates green.
- 2026-11-18 : phase 03 - the Diagnostics panel on the menu and pause screens, tested with a fake exporter, pictured through a render-texture panel. Gates green.
