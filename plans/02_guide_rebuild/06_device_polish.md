---
status: done
---

# Phase 06 - Polish from the device run

## Overview

The user, 2026-11-18, after playing the `dev` APK: "Change title from artificial pi to something more general" and "add a bunch of green blobs just to give some idea of moving".
On a flat untextured plane the avatar's movement is hard to see; a few static landmarks fix that.

## Plan

- `UI/Menu.uxml`: the menu title "Artificial Pi" becomes "Unity Setup", the product name.
- `PF_Blob`: a built-in sphere with a green URP Lit `M_Blob` (GPU instancing on), no collider. About 40 instances in `Game.unity` under a `Blobs` parent, static, at fixed pseudo-random positions (seeded) within 60 m of the origin, none within 6 m of the start, sizes 0.8 to 2 m, resting on the plane. Placed by a temporary editor method in batch mode.
- `docs/functional-specs.md` §5 (Game scene contents) names the blobs.

## Done when

- `Tools/gates.sh all` green; a capture shows the blobs; the `dev` APK builds.
- HUMAN: the title and the blobs on the phone.

## What the implementation found

- 2026-11-18. Menu title "Unity Setup". `PF_Blob` (sphere, no collider) with `M_Blob` (URP Lit, green, GPU instancing), 40 static instances under `Blobs` in `Game.unity`, seed 7, within 60 m, none within 6 m of the start, 0.8 to 2 m. Placed by a temporary editor method, deleted before the commit.
- The first capture showed the blobs, the ground and the capsule near black. Logging the scene's lights showed the directional light enabled as the sun, and a capture at three times its intensity was brighter, so the light works: the **first** `Camera.Render()` after opening a scene comes out with ambient light only. Rendering twice gives the lit frame (green blobs, grey-green ground, grey capsule).
  The dark ship in artificial-pirate's captures was the same artefact. Added to `headless-unity.md` §5.
- The temporary script was named after this plan's phase; the user, the same day: source files never mention plans or phases, temporary ones included. `headless-unity.md` §4 now says to name one-off scripts for what they do.
- Gates: `Tools/gates.sh all` green (0 warnings, 114/114, 4/4). `dev` APK `com.pitrified.unitysetup`, `Unity Setup`, 70 MB.
- Open: the HUMAN look on the phone.
- 2026-11-18, the user on the phone: "Lovely works."
