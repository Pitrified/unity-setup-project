---
status: planned
---

# Phase 05 - Ripples, splashes, shore

## Plan

- `WaterRipples`: a CustomRenderTexture (double-buffered wave equation, about 256x256 over a square around the avatar) the water shader samples for normals and foam; impulses added through update zones.
- `RippleSource`: moving floaters and the avatar in the water leave a wake; `Buoyancy` splashes on entry above a speed.
- `CannonballLauncher` (sample only): lobs a heavy ball into the water near the barrels every few seconds; splash, sink, knock a barrel.
- Shore lapping: foam bands travelling up the depth gradient, broken by noise.
- Tests: the ripple source rate-limits impulses; the launcher's ball sinks.

## Done when

- Gates green; a sequence shows a wake behind a moving floater, a splash ring spreading, foam moving at the beach.

## What the implementation found
