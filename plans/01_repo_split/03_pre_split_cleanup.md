---
status: done
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

- 2026-11-17, all in batch mode on the box; each commit closed on EditMode 114/114, PlayMode green and no compiler warnings.
- `453cba9` MCP pinned to `b92c05a2`, the commit `packages-lock.json` already resolved; no package change.
- `2b9d161` PlayMode tests shared the `DontDestroyOnLoad` `GameManager` across tests and passed only in their alphabetical order; the new overlay test, sorting first, broke the loop test. A `[UnityTearDown]` per fixture destroys it.
- `78df44a` overlay moved into `Persistent.unity` (PlayMode test added, 4/4); volume profile cleared on both URP assets (captures before and after differ only by the animated water, the same as two captures with no change); leftovers and the template's project-wide input actions deleted; no reference left to any deleted GUID; no LFS object left. companyName `Pitrified`, productName `Unity Setup` (my choice, not raised: nothing had named the guide), Android id `com.pitrified.unitysetup`.
- `ed02a2f` Q9 a: `ISaveSystem` is `Load<T>(version)` / `Save<T>(payload, version)` around an envelope the save system owns; `GameSaveData` and `ShipSaveData` live in `Game.Gameplay`, payload version 2, a version 1 file resets. `Game.Systems` has no mention of the ship.
- Copying a warm `Library/` from another clone gave "referenced script is missing" for every scene component in PlayMode; a fresh import fixed it. Do not copy `Library/` between clones.
- `dev` APK built with `UNITY_PATH` unset: the script found the editor, the APK is `com.pitrified.unitysetup`, label "Unity Setup", `arm64-v8a`, 73 MB. An interrupted build leaves `Assets/Resources/PerformanceTestRun*.json` behind (the performance test package's build step); delete it if a build is killed.
