# Water - implementation tracking

A middle-ground water for the template: shared Gerstner waves, baked shore depth, buoyancy, ripples, shore foam. Research and choice in [`00_start.md`](00_start.md).

## Key decisions

- **One wave function, two places**: the shader and `WaveField` read the same settings and the same clock.
- **Bake, don't copy depth**: the shore comes from a seabed depth map baked once, not URP's depth texture.
- **Judge it moving**: water is checked with frame sequences over time in Play mode, not single stills.

## Phases

| #  | Phase                          | Plan                                       | Status  |
| -- | ------------------------------ | ------------------------------------------ | ------- |
| 01 | Research and choice            | [`00_start.md`](00_start.md)               | done    |
| 02 | Capture over time              | [`02_sequence_capture.md`](02_sequence_capture.md) | done    |
| 03 | Waves, shading, island         | [`03_surface.md`](03_surface.md)           | done    |
| 04 | Floating                       | [`04_buoyancy.md`](04_buoyancy.md)         | done    |
| 05 | Ripples, splashes, shore       | [`05_interaction.md`](05_interaction.md)   | planned |
| 06 | Budget, docs, verify           | [`06_verify.md`](06_verify.md)             | planned |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-18 : started unsupervised; research written and the middle ground chosen (Gerstner shared with C#, baked seabed depth, buoyancy, ripple render texture, shore foam). Six phases.
- 2026-11-18 : phase 02 - sequence capture in Play mode (fixed steps, frames from a pose) and `measure.py sheet / gif / motion / track`; control measured still. In the docs and the skill.
- 2026-11-18 : phase 03 - Gerstner waves shared by shader and C#, baked seabed depth, island and sea in the sample scene, sky matched at the horizon. Three problems from the first still fixed; a shore sequence shows the water moving and the land still. Gates green.
- 2026-11-18 : phase 04 - `Buoyancy` with volume slices; the first model let bodies roll over and sink (found by the floating-props test and a logged run); barrels, crates and buoys float in the sample. Gates green.
