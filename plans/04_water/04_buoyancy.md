---
status: planned
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
