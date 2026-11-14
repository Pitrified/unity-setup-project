---
status: draft
---

# Phase 02 - Audit

## Overview

Every tracked file gets a destination, so the copy in phase 04 and the guide rebuild both work from a list rather than from judgement made in the moment.
Same method as the flutter audit, adapted to Unity's asset graph.
Context: [`00_start.md`](00_start.md), after [`01_land_ph6.md`](01_land_ph6.md).

## Goals

1. A table, `02.1_audit_table.md`, with one row per path pattern: `guide`, `pirate`, `both`, or `drop`, and a reason where it is not obvious.
2. A list of couplings and dead files, feeding phase 03.
3. The guide's gallery candidates and missing docs, feeding `02_guide_rebuild`.

## Plan

- List `git ls-tree -r --name-only main` and group by folder. A `.meta` follows its asset; `ProjectSettings/` and `Packages/` are `both`.
- Classify each code file by reading it, not by its folder name: `Game.Core` holds both the generic `SceneLoader` and the ship-aware `GameManager`.
- **GUID graph.** A script (in this repo's `scratch/`, not committed to `scripts/`) parses every `.meta` for its `guid`, then every `.unity`, `.prefab`, `.asset`, `.mat`, `.asmdef` and `.inputactions` for `guid:` references, and reports:
  - assets referenced by nothing (candidates for `drop`, starting with the template leftovers of Q10),
  - `drop` or `guide` assets referenced from a `pirate` asset, and the reverse.
- Docs: each doc gets a row. Most are `both` with edits; the functional spec and `systems/ship-controller.md`, `camera-controller.md` are `pirate`.
- `docs/tracking.md` phases and `scratch/` pads follow Q12.
- **Coverage check**, as in flutter: a script reads the backticked patterns in the table, matches every tracked path against them, and fails on an unmatched path or an empty pattern. Seen failing first on a misspelt pattern.
- Raise anything the audit decides that was not in the start file as new questions in `00_start.md`.

## Out of scope

- Changing any file outside `plans/`.

## Done when

- The coverage check passes with 0 unmatched paths and 0 empty patterns, after being seen to fail.
- The GUID report is summarised in the table file, with every cross-destination reference either resolved or raised as a question.

## What the implementation found
