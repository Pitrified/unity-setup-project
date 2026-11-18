---
status: done
---

# Phase 03 - Method doc and skill

## Overview

What phases 01 and 02 actually did, written as a method a session can follow for the next object.

## Plan

- `docs/building_assets.md`: when to build and when to take, what right looks like first, the passes, the probe, the Unity side (import, materials, prefab, capture, test), the budget, licences and credits, pitfalls, how a run ends.
- `.claude/skills/build-asset/SKILL.md` with its measuring script (`measure.py`: patch medians as ratios to a neutral patch, spread, silhouette ratios from a mask, side-by-side crops), run through the `uv` project.
- The skill tried on a second, small object start to finish (a wooden crate), to see that it can be followed without this session's context; what it gets wrong is fixed in the skill.

## Done when

- The doc and the skill exist, the crate went through them, the link check passes.

## What the implementation found

- 2026-11-18. `docs/building_assets.md` (build or take and the licences, references and the Commons API, the builder contract, the Unity side, the passes, budget, pitfalls, how a run ends) and `.claude/skills/build-asset/SKILL.md` (the loop in short). `measure.py` stays in `Tools/asset_builders/`, beside the builders it serves, rather than in the skill folder.
- **The crate**, run by the same session from the skill, so a test of whether the skill is complete rather than of whether a fresh session can follow it.
  Reference: a 1940s Swedish margarine crate (CC BY 4.0, R. Henrik Nilsson), whose Commons description gives the size, 27 x 26.5 x 13.5 cm. List: the size; boards about 1 cm, one wide board a face; a one-board lid overhanging about 1 cm front and 0.5 cm sides; weathered grey-tan wood; nails and stencil not modelled. One shape pass matched it; 72 triangles, one slot.
- **What the crate found that the skill lacked**, each now in the doc and the skill:
  - no neutral in both pictures: a **grey card**, `SceneCapture -captureGreyCard x,y,z`, against something grey in the photograph;
  - two different neutrals give **hue, not value**; `measure.py` now prints hue (the ratio over its mean) and its offset;
  - a "neutral" can be tinted (a mossy rock, `#bec5ad`): `"neutral_mode": "luminance"` in the spec trusts the photograph's white balance for hue;
  - **the patch must stand for the face**: a corner was browner than the face's median;
  - I first corrected against a hand calculation that skipped the render's sky cast; with the tool's numbers the hue landed within 0.003 in one more capture (four crate captures in all).
- **Flat colour vs grain** again: the crate's spread is about 1 against 87 in the photograph, and a weathered board's streaks make it read greyer than its median. Recorded as the texture upgrade path, not chased.
- `BarrelModelTests` became `BuiltModelTests`, one fixture over every built model (size, base-ring pivot, transforms, slots); a new model adds a case. EditMode 122/122.
- Gates: compile 0 warnings, EditMode 122/122, PlayMode 4/4. Link check: the 8 old diary links only.
