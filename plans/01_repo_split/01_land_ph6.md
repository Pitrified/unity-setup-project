---
status: draft
---

# Phase 01 - Land phase 6 work, adopt `plans/`

## Overview

The split starts from the latest work, which is on `origin/feat/ph6`, 8 commits ahead of `main` (Q4, Q5 in [`00_start.md`](00_start.md)).
This repo also gets the `plans/` convention this folder uses, so the audit and later phases have a place to live.

## Goals

1. `main` holds phases 6 to 8.
2. `plans/` is the place for new work, and the instructions say so.

## Plan

- HUMAN: fast-forward `main` to `origin/feat/ph6` and push, from g4. Branch `feat/01_repo_split` from the result; this folder moves onto it.
- Tick or carry over the three phase 8 HUMAN items in `docs/tracking.md`, per Q5.
- Add a short "Plans" section to `.github/copilot-instructions.md` and `AGENTS.md`: new work goes in `plans/NN_name/`; `docs/tracking.md` and `scratch/` are the earlier diary.
- Add `CLAUDE.md` importing `.github/copilot-instructions.md`, as the other repos do.
- Vendor `plans.py` from the tracked-development skill into `scripts/plans.py`, if you want the listing and checks here; otherwise skip it.

## Out of scope

- Moving or rewriting `docs/tracking.md` and `scratch/`: decided in `02_guide_rebuild`.

## Done when

- `git log main` shows `2c0c76b`.
- `plans/` is on `feat/01_repo_split` and the instructions point at it.

## What the implementation found
