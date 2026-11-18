---
status: planned
---

# Phase 03 - Waves, shading, island

## Plan

- `SO_WaterSettings` (four Gerstner waves: direction, wavelength, steepness, speed; colours; foam), `WaveField` (C#: displacement, height and normal at a point and time, no allocations), `WaterSurface` (pushes settings and the clock to the shader every frame; a clock that a capture can pin).
- `Water.shader`, hand-written URP, SRP-batcher compatible, transparent: Gerstner displacement in the vertex stage; seabed depth from the baked map for colour, foam and edge fade; generated normal map, two scrolls; Fresnel to sky colour; sun highlight; crest foam.
- Builders: `island.py` (a radial island with a beach sloping under the sea), `water_grid.py` (a grid mesh), `water_normals.py` (a tiling normal map PNG).
- `WaterDepthBake` editor tool: raycasts the seabed under the water area, writes the depth map.
- Scene: the flat ground replaced by the island and the sea; blobs kept on land.
- EditMode tests: `WaveField` against hand-computed Gerstner values, periodic in time, no allocation.

## Done when

- Gates green; a sequence shows waves travelling, colour deepening away from the shore, foam at the beach.

## What the implementation found
