---
status: done
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

- 2026-11-17. Root `README.md` (what the guide is, running it, the pattern index with artificial-pirate's ship controller and tuning, its water shader and its bootstrap folder, the history note) and `docs/bootstrap-new-project.md`, written from what artificial-pirate's bootstrap and this folder's phases 01 to 03 actually ran into.
- `functional-specs.md` is the sample's spec, keeping its shape, with a note that a new project keeps the shape and replaces the content; save schema v3 with the version history. `ship-controller.md` moved to `avatar-controller.md`; the camera, input, save and game manager docs follow.
  `game-manager.md` said "no ship, camera, or world references" while the code registers the ship; it now names the avatar as the one gameplay reference, by design.
- `performance-budget.md`, `build-and-release.md`, `google-play-private-alpha.md`, `testing-strategy.md`: the game's version label and game words gone; the release doc's template package id replaced with the guide's, and the manual loop drives the avatar.
- `headless-unity.md` points at `Tools/gates.sh` and gains what this folder found: rename with `.meta`, renamed serialized fields, `m_EditorClassIdentifier`, a moved `Library/`, an unusable `DISPLAY`, a RAM-backed `/tmp`.
- `copilot-instructions-template.md` deleted (Q5); the instructions, `AGENTS.md` and `docs/README.md` rewritten for the guide.
- Four history links broke with the rename and the deletion (two in `docs/tracking.md`, two in the diary); they now point at the files at `8b521a7` on GitHub. The link check is back to the 8 diary links that never resolved (`01_repo_split` finding 6).
- The paths artificial-pirate links resolve: `ai-development-playbook.md` with `## 3. The work loop`, `mcp-setup.md` with `## 2. Setup`, `dev-environment-setup.md`, the repo root.
- Game words left on purpose: the pattern index, the save schema's version history, the history note, "a water shader" as an example of `_Time`.
