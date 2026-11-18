---
status: planned
---

# Phase 01 - Recorder and report

## Plan

- `Log`: the ring in every build, 500 lines; the overlay reads the **newest** 20 (it read the oldest, harmless only while the ring held 20).
- `DiagnosticsRecorder` (in `Persistent.unity`): every frame, the frame time into a session histogram (0.5 ms bins) and a last-minute ring of frame times; every second, memory in use, managed heap and GC count into histograms and a 600-sample per-second series; exceptions from `Application.logMessageReceived`, distinct by message and first stack line, with count, first and last time, top stack frames, at most 10. No allocation per frame.
- `DiagnosticsReport`: builds the text, abyss's shape: header lines, a blank line, statistics, exceptions, the log lines; the detailed variant adds the series.
- `SO_BuildInfo`: commit and build time, written by `BuildPipeline` before each build; "editor" in the Editor.
- Fix the boot's quality lookup (`mobile` vs the project's `Mobile`).
- EditMode tests: quantiles from a histogram against a sorted array, the report's sections and order, the exception de-duplication and cap, the ring returning the newest lines.

## Done when

- Gates green; a report printed from a PlayMode run reads right.

## What the implementation found
