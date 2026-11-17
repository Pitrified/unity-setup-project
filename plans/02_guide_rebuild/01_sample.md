---
status: planned
---

# Phase 01 - Neutral sample

## Overview

Q1: the ship slice goes, a placeholder avatar takes its place, and the runtime under it stays as it is.
The ship controller is already a kinematic arcade mover with throttle and steering; it becomes the avatar's controller by renaming, not by rewriting.

## Plan

- `git mv`, so each `.meta` and its GUID move with the file: `ShipController` to `AvatarController`, `ShipState` to `AvatarState`, `SO_ShipTuning` (script and asset) to `SO_AvatarTuning`, `PF_Ship` to `PF_Avatar`, `ShipControllerTests` to `AvatarControllerTests`. Types, members, comments and log lines follow.
- `GameManager.RegisterShip` to `RegisterAvatar`, `_activeShip` to `_activeAvatar`; `GameSceneWiring` the same. `Game.Core` keeps referencing `Game.Gameplay`: the composition root knows the one thing it wires, which is the pattern a new project renames.
- Save: `ShipSaveData` to `AvatarSaveData`, `GameSaveData.ship` to `avatar`, `CurrentVersion` 3 (the field name is part of the schema; an old save resets, which the save system already handles).
- Assets, through a temporary editor method in batch mode, deleted before the commit:
  `PF_Avatar`'s mesh from the cube to the built-in capsule; a new URP Lit `M_Ground` on the sea plane, renamed `Ground`; the island instances removed from `Game.unity`.
- Deleted: `PF_Island`, `M_Island`, `M_Water`, `SG_Water.shader`, once the GUID graph shows nothing else references them.
- Tests: `GameManagerTests`, `PauseResumeTests`, `SaveRestoreTests` follow the renames; behaviour unchanged.

## Done when

- `git grep -i ship` finds nothing under `UnitySetupPrpj/Assets/`.
- Compile with 0 warnings, EditMode and PlayMode green (the same test count, renamed), and a capture of the Game scene showing the capsule on the ground plane.

## What the implementation found
