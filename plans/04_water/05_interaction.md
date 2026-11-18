---
status: done
---

# Phase 05 - Ripples, splashes, shore

## Plan

- `WaterRipples`: a ripple height field (wave equation, 256x256 over a fixed 48 m square) the water shader samples for normals and foam; impulses added as an array the step pass reads.
- `RippleSource`: moving floaters and the avatar in the water leave a wake; `Buoyancy` splashes on entry above a speed.
- `CannonballLauncher` (sample only): lobs a heavy ball into the water near the barrels every few seconds; splash, sink, knock a barrel.
- Shore lapping: foam bands travelling up the depth gradient, broken by noise.
- Tests: the ripple source rate-limits impulses; the launcher's ball sinks.

## Done when

- Gates green; a sequence shows a wake behind a moving floater, a splash ring spreading, foam moving at the beach.

## What the implementation found

- 2026-11-18. `WaterRipples` (two RGHalf render textures, ping-pong `Graphics.Blit` through `WaterRippleSim.shader`: the 2D wave equation, damped, impulses added from an array of up to 16 per step, absorbed at the border), `RippleSource` (a splash crater when entering fast from above, small rings while bobbing or moving; speed from position, so kinematic movers work), `CannonballLauncher` (sample: a pool of four iron balls on ballistic arcs every 3 s, sinking through their own `Buoyancy`). The water shader takes the field's slope (two extra taps) for normals and its height for foam; the waterline foam swashes up and down the beach.
- **First version on a CustomRenderTexture**, as the research said. In the game it worked; in a test it never stepped (both channels 0, shader and texture valid): its updates are queued and run during rendering, at a point the code does not control. Replaced by an explicit blit ping-pong, which steps when called; the test then passed. A known ceiling avoided rather than worked around.
- Also found on the way: the water shader declared the ripple texture as a material property, which shadows the global (removed); a first `LateUpdate` cleared the impulses before the texture updated in rendering (removed); a pooled ball's `RippleSource` remembered its sunk position when re-enabled (move first, then enable).
- **Too much splash** in the first sequence: a 12 cm ball made a white disc metres wide and rings across the whole bay for seconds. Splash strength per m/s 0.05 to 0.012, ripple foam 4 to 1.5, damping 0.985 to 0.97 per step (a ring halves in about 0.75 s). Second sequence: a brief splash, rings spreading and fading in 2 to 3 s, struck barrels bobbing.
- Tests, PlayMode: the ripple field read back from the GPU (an impulse lifts the water, a ring has moved outward 10 steps later, the energy is under a fifth after 150 more) and the launcher's ball lands near its target and sinks. Test isolation: earlier tests leave the Game scene loaded, so code-built tests now sit 1 km from its island (a ball had landed on it); the launcher puts its balls in its own scene (the boot loop test caught them leaking into the persistent scene); the floating-props test skips bodies denser than water.
- The avatar makes no wake: it walks at a fixed height that stays above the water. A wading avatar needs the controller to follow the ground; out of scope.
- Shore sequence: the waterline band widens and narrows along the beach between frames and the bands travel; motion in the beach row of the grid 0.4 to 2.6 per frame.
- Gates: compile 0 warnings, EditMode 127/127, PlayMode 12/12.
