---
status: done
---

# Phase 06 - Budget, docs, verify

## Plan

- Budget measured in the editor (triangles, draw calls, batches with `UnityStats` around a capture) against `docs/performance-budget.md`.
- `docs/systems/water.md` (how it works, how to add water to a scene, what it costs), the spec's scene table, the README.
- A fresh clone on disk: builders regenerate identical files, `Tools/gates.sh all`, `Tools/build-android.sh dev`.
- HUMAN, later: the APK on the phone, frame rate by eye, the look.

## Done when

- Box checks green in a fresh clone; docs written.

## What the implementation found

- 2026-11-18. **Budget**: the Editor's render counters (`UnityStats`) read 0 for a scripted render in batch mode, so `SceneCapture -captureBudget` counts from the scene (renderers in view, their triangles and material slots). The game's start 56.7 k triangles and 30 slots, over the bay 49.0 k and 16, above the island 63.7 k and 39; budget 150 k and 80. The water grid is 33.8 k of it.
- **Docs**: `docs/systems/water.md` (parts, rules, adding water, making something float, cost, checking), the spec's scene and systems tables, the docs index, the README, and in `headless-unity.md` the budget flag and three pitfalls (render counters, CustomRenderTexture timing, tests sharing the world).
- **Fresh clone** of `feat/04_water` at `fabf057` under `~/.cache/`: `uv sync`; barrel, crate, island and water grid FBX and the normal map regenerate byte-identical (git status clean); `plans.py check`; link check with the 8 old diary links only; `Tools/gates.sh all` from an empty `Library/`: compile 0 warnings, EditMode 127/127, PlayMode 12/12; `dev` APK 70 MB, `com.pitrified.unitysetup`. Copied to the working repo's `UnitySetup/Build/dev/` for the phone.
- Open, HUMAN: the APK on the phone (frame rate by eye, the look of the water, the cannon's splashes).
