# 📄 `docs/systems/water.md`

## Overview

Water for a mobile game: Gerstner waves that floating things ride, shore colour and foam from a baked depth map, ripples from splashes and bobbing.
Decent and reactive, not photoreal; one transparent draw for the surface, no depth or opaque texture, a 256x256 ripple step per frame.
The choice and the alternatives are in `plans/04_water/00_start.md`.

---

## Parts

| Part | Where | What it does |
| ---- | ----- | ------------ |
| `SO_WaterSettings` | `Game.Gameplay.Water`, `Assets/Settings/SO_WaterSettings.asset` | Sea level, four Gerstner waves, colours, foam, detail normals, calm depth |
| `WaveField` | `Game.Gameplay.Water` | The Gerstner sum on the CPU: displacement, height at a world point, normal; no allocations |
| `WaterSurface` | on the `Water` object | Owns the `WaveField` and the clock; hands the shader the waves, clock, colours and depth map as globals every frame; runs in the Editor too |
| `Water.shader` | `Assets/Shaders/` | Vertex: the same Gerstner sum, calmed in the shallows. Fragment: shallow-to-deep colour, detail normals, Fresnel to the sky, sun glint, foam (waterline swash, shore bands, crests, ripples), no water over land |
| `WaterDepthBake` | `Game.Editor` | Casts rays down over the water's area, writes `T_WaterDepth.png` (0 at the surface, 1 at the depth range) |
| `Buoyancy` | on a Rigidbody | Floats it on a `WaterSurface` by its volume and density |
| `WaterRipples` | on the `Water` object | A wave-equation height field over a fixed square, stepped by blitting between two render textures (`WaterRippleSim.shader`) |
| `RippleSource` | on anything that touches water | A splash on a fast entry, small rings while it bobs or moves |
| `CannonballLauncher` | sample only | Lobs iron balls at the floaters every few seconds |

---

## Rules

- **One wave function.** The shader's `GerstnerSum` and `WaveField.Displacement` must stay the same sum; a change to one is a change to both, and `WaveFieldTests` checks the C# side against the formula.
- **One clock.** Everything reads `WaterSurface.Now`; a capture or a test can pin it (`PinClock`).
- **The shader reads globals**, so a scene has one body of water at a time.
- **Ripples are visual.** Floaters ride `WaveField` only; a splash does not push them (the ball hitting them does, through physics).
- **The drawn waves calm in water shallower than `CalmDepth`; the CPU waves do not.** A floater in very shallow water rides a little high. Keep floaters in deeper water, or add the depth map to `WaveField` if it matters.

---

## Adding water to a scene

1. A ground with colliders under the water (`ModelSetup -colliders` for a model), and a water grid mesh (`Tools/asset_builders`: `build.py water_grid`) with the `M_Water` material, at the origin.
2. `WaterSurface` on the grid, with an `SO_WaterSettings`; set its depth rect to cover the area.
3. Bake: `-executeMethod Game.Editor.WaterDepthBake.BakeFromCommandLine` (`docs/headless-unity.md`); bake again when the shore changes.
4. Optional: `WaterRipples` on the same object with `M_WaterRippleSim`, centred where things happen.
5. A procedural sky whose ground colour is the far water's, or the horizon shows a band past the grid's edge.

## Making something float

- A Rigidbody, a collider, `Buoyancy` with the body's **volume**, its **density relative to water** (under 1 floats) and **sample points**.
- Each point is the centre of an equal slice of the volume, `columnHeight` tall. **Spread them through the whole body, top and bottom**: points only on the lower half make it roll over and sink once they end up on top. A box: its eight octant centres. A barrel: two rings at the centres of its halves. A sphere: the corners of its inner cube.
- The draft follows from the density: a 1 m cube of density 0.5 floats with its centre at the waterline (`BuoyancyTests`).
- Do not start a body overlapping a one-sided mesh collider (a beach): it can be pushed through to the underside.
- A `RippleSource` with the water and the ripples gives it splashes and rings.

---

## Cost

Measured in the sample scene with `SceneCapture -captureBudget` (renderers in view, before batching), 2026-11-18:

| View | Triangles | Material slots |
| ---- | --------- | -------------- |
| The game's start | 56.7 k | 30 |
| Over the bay | 49.0 k | 16 |
| Above the island, nearly everything | 63.7 k | 39 |

Against `docs/performance-budget.md`: 150 k triangles and 80 draws. The water grid is 33.8 k of it (130 x 130 cells of 2 m, so the shortest wave should stay at 3 m or more). On the GPU per frame: one 256x256 ripple step, the surface's fragment work (five texture reads, a handful of sin/cos in the vertex stage). The device run is what settles it.

---

## Checking it

- `WaveFieldTests` (EditMode), `BuoyancyTests`, `FloatingPropsTests`, `WaterRippleTests` (PlayMode).
- Look at it moving, not still: the sequence capture and `measure.py sheet / gif / motion / track` (`docs/headless-unity.md` §3).
