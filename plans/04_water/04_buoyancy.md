---
status: done
---

# Phase 04 - Floating

## Plan

- `Buoyancy`: on a Rigidbody, sample points in local space; each submerged point pushes up in proportion to its depth under `WaveField`; linear and angular drag while wet; a density setting so heavy things sink.
- Barrels and crates moved into the water near the shore; sea blobs become green buoys.
- Parity: the floating barrel sits on the surface the GPU draws; checked in a sequence capture (no gap, no sinking through).
- Tests: a body released above calm water settles at its draft; a dense one sinks; the PlayMode harness checks barrels stay afloat for a few seconds.

## Done when

- Gates green; a sequence shows barrels bobbing in step with the waves under them.

## What the implementation found

- 2026-11-18. `Buoyancy` on a Rigidbody: mass from a density relative to water and a volume; sample points, each the centre of an equal slice of the volume, wet from half a slice below the point to half above, sampled on `WaveField` at the water's clock; damping rises with the wet fraction.
- **First model, wrong**: points were columns standing on their point, and the scene's buoys had all four points on their lower half. Three large buoys sank in the sequence. The floating-props test logged them 1.3 to 1.4 m under the surface with every point wet; logging one over time showed it bob normally for 2 s, roll over, and lose all lift with its points on top. Lift applied only below the centre of mass is unstable; spread through the volume it follows where each slice is, whatever the orientation. The doc comment now says to spread the points.
- Also seen: blobs placed over the beach slope sank through the island's one-sided mesh collider when they started overlapping it; the nine between 24 and 42 m out were removed, buoys are only past the beach.
- `BuoyancyTests` (PlayMode, calm water built in code): a 1 m cube of density 0.5 settles centred on the surface, 0.8 floats 0.3 m deeper, a cube released upside down at 170 degrees still floats, density 2 sinks. `FloatingPropsTests` boots the game and checks every floater is within 1.2 m of the surface after 6 s, logging the ones that are not (that log found the roll-over).
- Scene: three barrels (two lying), two crates and 21 buoys (blobs past the beach) float off the beach. The internal setters `SO_WaterSettings.SetWaves` and `WaterSurface.UseSettings` let tests build water in code.
- **Sequence**, 6 s at 5 fps from the beach: barrels roll and bob, crates ride the swell, the big buoy's centre moves over 29 px with a period of about 3 s in step with the waves. `measure.py track --colour` was added for it: the bright-pixel centroid does not follow a green buoy on bright water.
- Gates: compile 0 warnings, EditMode 127/127, PlayMode 10/10.
