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

- Before dropping `SampleScene`, move the debug overlay into `Persistent.unity`: a GameObject with `UIDocument` (`DebugOverlay.uxml`, `SO_DebugOverlayPanel.asset`) and `DebugOverlayController`, as `scratch/scratch-pad-ph04.1.md` intended ([audit](02.1_audit_table.md) finding 1). Done by an editor method in batch mode; checked by a PlayMode test that the overlay object exists after boot.
- Set the volume profile of `Mobile_RPAsset` and `PC_RPAsset` to none, so `SampleSceneProfile` is referenced by nothing (finding 2). A headless capture of the Game scene before and after shows no change.
- Clear the project-wide input actions in `EditorBuildSettings` and delete `Assets/InputSystem_Actions.inputactions` (finding 3).
- Drop the template leftovers the GUID report shows unreferenced (Q10): `Assets/TutorialInfo/`, `Assets/Readme.asset`, `SampleScene.unity`, `SampleSceneProfile.asset`, with their `.meta` files. `.gitattributes` keeps its LFS rules although no LFS object is left: LFS stays as the pattern for the next binary.
- Pin `com.coplaydev.unity-mcp` to the commit `packages-lock.json` records (Q11).
- Set `companyName`, `productName` and `applicationIdentifier` in `ProjectSettings.asset` for the guide (Q8). Set by Unity, not by hand, so it writes the rest of the file consistently: a `PlayerSettings` call from a batch-mode editor method on the box.
- If Q9 is a: make `SaveSystem` store a versioned payload whose game section is owned by `Game.Gameplay`; update `SaveSystemTests`, `GameManager` and `SaveRestoreTests`.
- Anything else the audit lists as dead.

## Out of scope

- Turning the sample game into a neutral one: that is `02_guide_rebuild`.

## Done when

- On the box, in batch mode: the project compiles with 0 errors and 0 warnings, EditMode tests green, PlayMode tests green under `xvfb-run ... -force-vulkan`.
  The PlayMode tests cover Boot, Menu, Game, Pause, Resume, Menu, Continue restoring the ship.
- The GUID script reports no dangling references.

## What the implementation found
