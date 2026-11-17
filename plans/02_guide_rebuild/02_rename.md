---
status: planned
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
