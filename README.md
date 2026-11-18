# Unity Setup

A guide to starting a Unity 6 mobile (Android) project that an AI agent can build and check on its own: a small runtime to start from, the rules and docs that go with it, and links to projects that implement a pattern worth copying.
It is not a gallery of samples; a pattern is linked from the project that uses it.

## What is here

- `UnitySetup/` - the Unity project (`6000.4.4f1`, URP, IL2CPP, ARM64): a generic runtime (boot, persistent systems scene, scene loading, input, save, audio, UI Toolkit screens, logging and a debug overlay) and a placeholder sample over it, a capsule avatar on a ground plane with a chase camera, so every system runs on a device.
- `Tools/` - `setup.sh` once per machine, `gates.sh` for the checks, `build-android.sh dev|profile|release`, the pre-commit hook, `plans.py`, and `asset_builders/`: Blender scripts that build the sample's barrel and crate, exported as FBX ([`docs/building_assets.md`](docs/building_assets.md), skill `.claude/skills/build-asset/`).
- `docs/` - start at [`docs/README.md`](docs/README.md).
- `plans/` - this repo's own work, one folder per feature.

## Starting a project from it

[`docs/bootstrap-new-project.md`](docs/bootstrap-new-project.md): plan first, copy with `git archive`, rename, docs, verify in a fresh clone.
[artificial-pirate](https://github.com/Pitrified/artificial-pirate) was started this way.

## Running it

Agents and people run the same gates, in batch mode with no Editor open:

```
Tools/setup.sh             # once per clone
Tools/gates.sh all         # compile with 0 warnings, EditMode and PlayMode tests
Tools/build-android.sh dev # the dev APK
```

Install and the pitfalls: [`docs/headless-unity.md`](docs/headless-unity.md).
At a workstation, open `UnitySetup/` in the Unity Hub at the version above and play from `Assets/Scenes/Boot.unity`.
Agents start from [`CLAUDE.md`](CLAUDE.md), which imports [`.github/copilot-instructions.md`](.github/copilot-instructions.md).

## Patterns in other projects

| Pattern | Where |
| ------- | ----- |
| Kinematic arcade ship: throttle and steering with smoothing, tuned through a ScriptableObject | artificial-pirate [`ShipController.cs`](https://github.com/Pitrified/artificial-pirate/blob/main/ArtificialPirate/Assets/Scripts/Gameplay/ShipController.cs), [`SO_ShipTuning.cs`](https://github.com/Pitrified/artificial-pirate/blob/main/ArtificialPirate/Assets/Scripts/Gameplay/SO_ShipTuning.cs), spec [`ship-controller.md`](https://github.com/Pitrified/artificial-pirate/blob/main/docs/systems/ship-controller.md) |
| Stylized URP water: a hand-written shader with animated waves | artificial-pirate [`SG_Water.shader`](https://github.com/Pitrified/artificial-pirate/blob/main/ArtificialPirate/Assets/Shaders/SG_Water.shader) |
| Bootstrapping a project from this guide | artificial-pirate [`plans/01_bootstrap/`](https://github.com/Pitrified/artificial-pirate/blob/main/plans/01_bootstrap/00_start.md) |

The sample's avatar controller is the ship controller with a capsule for a hull; the tuned original is the one to copy for a vehicle.

## History

The runtime and the naval game were built here in 2026, then the game moved to artificial-pirate (`plans/01_repo_split/`).
[`docs/tracking.md`](docs/tracking.md) (the setup checklist) and `scratch/` (a diary per phase) are that history, kept as they were; new work is in `plans/`.
