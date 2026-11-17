---
status: planned
---

# Phase 04 - Docs

## Overview

The docs as a guide: what this repo is, how to start a project from it, the rules and workflow, distribution. Game content goes or is linked to artificial-pirate.

## Plan

- Root `README.md`: what the guide is, the sample, how to run it (`Tools/gates.sh`), the pattern index (Q7) with artificial-pirate's ship controller and tuning ScriptableObject and its water shader, the history note (Q10).
- `docs/bootstrap-new-project.md` (Q6): plan folder first, `git archive`, the renames (folder, ids through `PlayerSettings` in batch mode, names), docs, gates in a fresh clone on disk, the human steps; artificial-pirate's `plans/01_bootstrap/` as the worked example.
- `docs/functional-specs.md` rewritten as the sample's spec (Q3), keeping the shape; `docs/systems/ship-controller.md` becomes `avatar-controller.md`, `camera-controller.md` stays, the rest follow the phase 01 renames.
- `performance-budget.md`, `build-and-release.md`, `google-play-private-alpha.md` made generic (Q4).
- `docs/copilot-instructions-template.md` deleted (Q5).
- `docs/README.md`, the instructions, `AGENTS.md`, `headless-unity.md` (pointing at `gates.sh`), `dev-environment-setup.md`, `testing-strategy.md`, `project-structure.md` to match.

## Done when

- Link check passes outside the history files; the four paths artificial-pirate links resolve, anchors included.
- `git grep -i "ship\|island\|water\|sea"` in `docs/` outside the history finds only the pattern index's links and words used generically.

## What the implementation found
