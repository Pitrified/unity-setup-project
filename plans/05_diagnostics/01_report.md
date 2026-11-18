---
status: done
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

- 2026-11-18. `Log`: the ring in every build, 500 lines (`Info`/`Verbose` still compiled out of Release); reads return the **newest** entries. The old read returned the oldest, and its test asserted that while its comment said "newest"; test and code now agree with the comment.
- New in `Game.Systems`: `Histogram` (fixed bins, exact min, max and mean, quantiles to a bin), `ProblemLog`, `SO_BuildInfo` (commit, time, kind, version code), `DiagnosticsRecorder` (in `Persistent.unity`), `DiagnosticsReport`. `BuildPipeline` stamps the build info around `BuildPlayer` (`git rev-parse --short HEAD`, `+dirty` with changes) and sets it back to "editor" in a `finally`, so a build leaves git clean.
- **Quality bug fixed**: boot looked for quality level `mobile`, the project names it `Mobile`; the report now shows `quality Mobile` and the warning is gone.
- **Lost reference, cause found**: the step that placed the recorder saved `_build: {fileID: 0}`; the asset was loaded before `OpenScene`, which unloads unused assets. Loading after opening fixed it. The water's earlier case was the same; `headless-unity.md` now says "open the scene first, then load what you assign".
- A real report from the running game (`DiagnosticsReportTests` with `REPORT_OUT`): 1.9 kB with the detailed series; header, both statistics blocks, no problems, the boot's 15 log lines, three seconds of series. In the Editor the managed heap reads about 890 MB (the Editor's own); the phone's number is the one that matters.
- Tests: `DiagnosticsTests` (EditMode: quantiles within a bin of exact over 5000 samples, empty and overflow, problem de-duplication and cap, report sections in order, detail only on request), `DiagnosticsReportTests` (PlayMode, the running game). `docs/systems/logging.md` has a "Diagnostics report" section.
- Gates: compile 0 warnings, EditMode 131/131, PlayMode 13/13.
