---
status: done
---

# Phase 02 - Folder and names

## Overview

Q2: `UnitySetupPrpj/` becomes `UnitySetup/`, and the game's names still here (`artificial-pi` in the build script, `setup.sh`, `build.env.example`, `BuildPipeline`, the camera tuning menu, the release doc) become the guide's, as artificial-pirate's phase 02 did the other way.

## Plan

- `git mv UnitySetupPrpj UnitySetup`; scripts and docs follow. `Library/` moves with it.
- `artificial-pi` to `unity-setup`: `~/.config/unity-setup/build.env`, the APK name, `BuildPipeline.ProductName`, the `CreateAssetMenu` paths.
- `ProjectSettings.asset`'s Windows Store package name and description through `PlayerSettings` in batch mode, as artificial-pirate did. The Unity Cloud `projectName` stays (read-only, tied to the cloud link).
- Not renamed: `docs/tracking.md`, `scratch/` and the `plans/` of `01_repo_split` (history, Q10), and the four paths artificial-pirate links.

## Done when

- `git grep` finds no `UnitySetupPrpj` and no `artificial-pi` (whole word) outside the history files and `plans/`.
- Compile with 0 warnings, EditMode and PlayMode green; the `dev` APK named `unity-setup-dev.apk`, id `com.pitrified.unitysetup`.

## What the implementation found

- 2026-11-17. `git mv UnitySetupPrpj UnitySetup`; `artificial-pi` to `unity-setup` in the build and setup scripts, `build.env.example`, `BuildPipeline`, both tuning menus, the instructions, `AGENTS.md` and three docs; `cd artificial-pi` in the dev environment doc became `cd unity-setup-project`.
  Windows Store package name and description through `PlayerSettings.WSA` in batch mode. Left on purpose: `projectName: UnitySetupPrpj`, the Unity Cloud link's name.
- **A moved project needs a fresh `Library/`.** With the `Library/` that moved along with the folder, EditMode passed but PlayMode failed 2 of 4: the Game scene's `AvatarController` was a missing script ("The referenced script (Unknown) on this Behaviour is missing!"). Deleting `Library/` and importing again fixed it, with no change to any tracked file.
  artificial-pirate's rename did not show it, its `Library/` being minutes old; the bootstrap how-to says to delete `Library/` after moving the project folder.
- Gates on the fresh `Library/`: compile 0 warnings (after the one-time Shader Graph import errors), EditMode 114/114, PlayMode 4/4.
- `dev` APK: `UnitySetup/Build/dev/unity-setup-dev.apk`, `com.pitrified.unitysetup`, label `Unity Setup`, `arm64-v8a`.
