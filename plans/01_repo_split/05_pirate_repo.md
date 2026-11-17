---
status: draft
---

# Phase 05 - Write artificial-pirate

## Overview

The new repo is written from a plan that lives in it, `plans/01_bootstrap/`, so a session working there needs nothing from this conversation.
Same order as `fala-language-tutor`: plan first, then the copy, then the cuts, docs, moved plans, verification.
Context: [`00_start.md`](00_start.md), after [`03_pre_split_cleanup.md`](03_pre_split_cleanup.md); who does what is Q13.

## Goals

1. `Pitrified/artificial-pirate` runs the game as it runs here, with nothing from the template it does not use.
2. Its open roadmap is in its own `plans/`.

## Plan

Written into the new repo as `plans/01_bootstrap/` with these phases:

- **Baseline.** `git archive` of this repo's `main` at the commit after phase 03, the rows marked `pirate` and `both`. Pull LFS objects first so no pointer file is copied. Name the source commit in the bootstrap `00_start.md`.
- **Renames** (Q2, Q7, Q8): project folder, `productName`, `applicationIdentifier`, `artificial-pi` to `artificial-pirate` in docs, scripts and `~/.config/<name>/build.env`. `ProjectSettings.asset` edited in the Editor.
- **Docs.** README, `CLAUDE.md`, instructions and `AGENTS.md` about the game. Generic docs that stay in the guide (AI playbook, MCP guides, dev environment from zero) become links to it on GitHub. A fresh-session note: how a session runs Unity headless (batch mode, `xvfb-run ... -force-vulkan` to render) and what stays with a person.
- **Plans.** Folders from `docs/tracking.md` phases 9 to 13 (Q12), numbered from 02, each linking its source.
- **Verify.**
  - Link check passes.
  - On the box, from a fresh clone: `git lfs ls-files` lists the binaries, `Tools/setup.sh` runs, the project compiles with 0 errors and 0 warnings, EditMode and PlayMode tests green, `Tools/build-android.sh dev` builds with the new id.
  - HUMAN: the APK installs next to any build from this repo (different id), and the vertical slice plays on the device.

## Out of scope

- New gameplay. The game behaves as it does here.
- Rebuilding this repo into the guide: `02_guide_rebuild`.
- CI or a headless Unity gate; a candidate for the guide.
- The asset-building method, `03_asset_method`; artificial-pirate copies it from the guide when it lands, and keeps its own findings doc for the ship.

## Done when

- The new repo's bootstrap folder is `done`, with the device run recorded in its log.
- This repo's log names the new repo's merge commit.

## What the implementation found
