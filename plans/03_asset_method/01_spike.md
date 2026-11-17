---
status: done
---

# Phase 01 - Pipeline spike

## Overview

The round trip, end to end and headless, before any method is written down: a builder script makes a barrel in Blender, exports FBX, Unity imports it, a test checks its axes and size, a capture shows it in the game scene.

## Plan

- `Tools/asset_builders/`: a `uv` project pinned to `bpy` (Python 3.13) and `numpy`; `build.py <name>` runs a builder from `builders/` and writes `UnitySetup/Assets/Art/Models/<Name>.fbx`.
- `builders/barrel.py`: a parametric barrel drafted blind from real dimensions (a 225 l barrique: about 0.95 m tall, 0.56 m at the heads, 0.72 m at the bilge), staves and hoops as two material slots, the origin at the base centre, metres.
- FBX export for Unity: Y up, -Z forward, units applied, so the mesh imports at scale 1 with no rotation on the root.
- Unity: the model imported with materials remapped to two URP Lit materials made in Unity; `PF_Barrel` with a collider; three barrels in `Game.unity` near the start.
- A permanent capture method in `Game.Editor` (named for what it does), rendering twice (`headless-unity.md` §5), callable with `-executeMethod` and arguments for the scene, camera position and output.
- An EditMode test on the imported mesh: height and bilge diameter within 1 cm of the builder's numbers, base at y 0, upright.
- FBX goes in LFS (already in `.gitattributes`).

## Done when

- `uv run` builds the FBX from nothing; `Tools/gates.sh all` green with the new test; a capture shows the barrels in the scene.

## What the implementation found

- 2026-11-18. `Tools/asset_builders/`: `uv` project (`bpy` 5.2.1 on Python 3.13, from the uv cache house-renderer filled), `build.py` (builder, FBX export, stats, probe), `probe.py` (Cycles on OptiX, the Quadro; one sun, uniform sky, grey ground, framing from the bounds), `builders/barrel.py`.
- Blind draft: 1676 triangles, two slots, 0.95 m by 0.723 m, base at 0. The probe shows a barrel at first render; the hoops read as round tubes, not flat bands (a pass for phase 02).
- **FBX rebuilds were not byte-identical** (about 50 bytes): the exporter stamps `datetime.now()` and derives object ids from Python's `hash()` of names, randomised per process. `build.py` re-executes itself with `PYTHONHASHSEED=0` and patches the exporter's clock; two builds now compare equal, so git only sees real changes.
- Unity side, permanent and named for what they do: `ModelSetup` (import settings for a static prop; each FBX material slot remapped to `Assets/Art/M_<slot>.mat`, created if missing; the remap lives in the `.meta`, so a rebuilt FBX keeps it) and `SceneCapture` (a scene's camera to a PNG, optionally moved, rendered twice).
- A temporary editor method set the two colours, made `PF_Barrel` with a capsule collider and placed three barrels under `Props` in `Game.unity`; deleted before the commit.
- `BarrelModelTests`: size, pivot at the base centre, no rotation or scale on any transform, both slots mapped. Passed first time; seen failing on a 1.2 m barrel (size), with the remap surviving that rebuild.
- Gates: compile 0 warnings, EditMode 118/118, PlayMode 4/4. Capture: barrels upright on the ground beside the avatar.
