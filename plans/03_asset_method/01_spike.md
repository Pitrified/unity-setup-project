---
status: planned
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
