---
status: draft
---

# Phase 03 - Clean up before duplicating

## Overview

Fix here, once, what would otherwise be copied into both repos.
Scope is whatever the audit and Q8 to Q11 in [`00_start.md`](00_start.md) settle; the list below is the starting point.
Context: after [`02_audit.md`](02_audit.md).

## Goals

1. No template leftovers, no unpinned package, no Unity template `applicationIdentifier`.
2. The couplings Q9 decides to fix here are fixed.

## Plan

- Drop the template leftovers the GUID report shows unreferenced (Q10): `Assets/TutorialInfo/`, `Assets/Readme.asset`, `SampleScene.unity`, `SampleSceneProfile.asset`, with their `.meta` files.
- Pin `com.coplaydev.unity-mcp` to the commit `packages-lock.json` records (Q11).
- Set `companyName`, `productName` and `applicationIdentifier` in `ProjectSettings.asset` for the guide (Q8). Edited in the Editor, not by hand, so Unity writes the rest of the file consistently.
- If Q9 is a: make `SaveSystem` store a versioned payload whose game section is owned by `Game.Gameplay`; update `SaveSystemTests`, `GameManager` and `SaveRestoreTests`.
- Anything else the audit lists as dead.

## Out of scope

- Turning the sample game into a neutral one: that is `02_guide_rebuild`.

## Done when

- HUMAN: the Editor opens with 0 errors and 0 warnings, EditMode and PlayMode tests all green in the Test Runner.
- HUMAN: Boot, Menu, Game, Pause, Resume, Menu, Continue restores the ship, in Play mode.
- The GUID script reports no dangling references.

## What the implementation found
