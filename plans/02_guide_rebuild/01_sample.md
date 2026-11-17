---
status: done
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

- 2026-11-17. Ship files moved to avatar names with their `.meta` files; GUIDs kept, so `Game.unity`'s prefab instance and the tuning reference survived.
- A renamed serialized field loses its value on the next save: `GameSceneWiring._ship` to `_avatar` came back `{fileID: 0}` after the scene was saved, and was re-linked through `SerializedObject`.
  `_seaSurfaceY` to `_groundY` lost nothing only because it is set in the same step. A new project renaming the sample hits this too; the bootstrap how-to (phase 04) says to re-link or use `[FormerlySerializedAs]`.
- Unity keeps a component's `m_EditorClassIdentifier` (the old class name) when the script is renamed under the same GUID; `ForceReserializeAssets` does not refresh it. Set through `SerializedObject.FindProperty("m_EditorClassIdentifier")`, which works.
- The capsule stands on the plane with `_groundY` 1 (half its height). `SaveRestoreTests` compared against a saved y of 0; it now compares with the position actually saved after the teleport, which is what it means to test. Seen failing (delta 1.0 m) before the change.
- Kept on purpose: the save schema's version history (`GameSaveData` comment, v1 had `ship`) and the legacy-file JSON in `SaveSystemTests.Load_ResetsFileWithoutEnvelope`.
- `PF_Island`, `M_Island`, `M_Water`, `SG_Water` deleted after checking that only they referenced each other; `Shaders/` kept with a `.gitkeep`. New `M_Ground` (URP Lit).
- Gates: EditMode 114/114, PlayMode 4/4, compile 0 warnings after the temporary script was removed. Capture: the capsule on the ground plane under the chase camera.
