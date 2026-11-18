---
status: done
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

- 2026-11-18. Built: `SO_WaterSettings`, `WaveField`, `WaterSurface` (in `Game.Gameplay.Water`), `Water.shader` (hand-written URP, transparent, one pass), `WaterDepthBake` (editor), builders `island.py` (7.6 k triangles, Grass and Sand slots, mesh colliders at import through a new `ModelSetup -colliders`), `water_grid.py`, and `textures.py water_normals` (a tiling normal map from integer-frequency waves, byte-identical on a rerun).
- `WaterSurface` hands the shader everything as globals (waves, clock, colours, depth map), so the material holds only textures; a capture can pin the clock.
- **Lost asset reference**: the scene step created the settings asset, then reimported a texture, and the reference it held saved as `{fileID: 0}`: the reimport replaced the in-memory asset. Loading it again from its path just before assigning fixed it; worth a line in `headless-unity.md` §4.
- Depth bake: 512 x 512 over 180 m, 78 % of rays hit ground (a 90 m disc in a 180 m square is 78.5 %).
- **First still, three problems**: (1) waves summing to 0.88 m rose over the island top, and the waterline foam painted white bands across the grass wherever the grid poked through; (2) the sea glittered white (too much crest foam, strong detail normals feeding Fresnel); (3) a brown band at the horizon, the default skybox's ground colour past the water grid.
  Fixes: waves of about 0.4 m in total; in the vertex stage the waves calm over water shallower than 2 m (depth map read with `SAMPLE_TEXTURE2D_LOD`); fragments over land are clipped; crest foam only on the top 15 % of a crest; Fresnel on a mostly geometric normal; grid 180 to 260 m (130 cells, 2 m quads, 33.8 k triangles, so the shortest wave became 3.4 m); a procedural sky whose ground colour is the far water's, measured from a capture (0.51, 0.64, 0.72).
- The CPU field does not calm in the shallows: a floater in very shallow water rides a little high. Said in `SO_WaterSettings.CalmDepth`'s doc; an upgrade if it shows.
- **Sequence**, 3 s at 10 fps from the shore: change per frame about 3 on open water, 0.4 to 1.1 near the beach (calmed), 0 on land and sky. Shore foam bands move but little (the bright centroid shifts about 3 px in 3 s): phase 05 strengthens the lapping.
- `WaveFieldTests` (5): the Gerstner formula at two phases, the height lookup against the forward displacement, travel at the deep-water speed, flat water normal, no allocation. Seen failing with the height iterations set to 0.
- `M_Ground` deleted (nothing references it after the ground plane went). The sea blobs stand in the water until phase 04 makes them buoys.
- Gates: compile 0 warnings, EditMode 127/127, PlayMode 5/5.
