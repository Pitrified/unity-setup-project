---
name: build-asset
description: >-
  Build a game object (a prop, a piece of a ship or a building) from a reference
  photograph: a Blender builder script in Tools/asset_builders exports FBX, Unity
  imports it, and passes are measured against the photograph until it reads the
  same, stylized. Headless and unattended. Use when asked to "make a barrel / crate
  / mast / ...", to model something "like this photo", to find references for an
  object, or to iterate on a model's shape or colours.
---

# Build an asset

The full method is `docs/building_assets.md`; this is the loop. The worked example is `builders/barrel.py` with `references/barrel/`.

## Before the first render

1. **References**: Commons by category with licences in one API call (the URL is in the doc, §2). Thumbnails, one at a time with a pause. Keep CC0, public domain, CC BY; never BY-SA, NC, ND. Put the chosen one in `Tools/asset_builders/references/<name>/` and a row in `references/CREDITS.md`.
2. **Real numbers** for that kind of object, from a sheet or a standard. The photograph gives ratios; the numbers give metres.
3. **Write what right looks like**: numbered, from the reference, before any render. It is the stop condition.

## The builder

- `builders/<name>.py`: `PARAMS` (units, sources) and `build(params)`; metres, Z up, origin at the base centre, -Y toward the viewer; material slots named, shared by name across models.
- Draft the whole object blind, then probe it: `uv run --no-sync python build.py <name> --no-fbx --probe r1.png --view AZ EL` (run `uv sync` once in `Tools/asset_builders`).
- Flat parts on a smooth mesh need `sharp_edge` on their rings.

## The loop

One class of change per pass; scratch files `r<N>`, never over a kept file; look at the reference every pass.

1. **Shape** in the probe. Measure: `measure.py profile` (bands and parts show as chroma or brightness steps), `measure.py crop` side by side. Average symmetric ends to cancel perspective.
2. **Into Unity**: `build.py <name>` writes the FBX; `ModelSetup.SetUpFromCommandLine -modelPath Assets/Art/Models/<Name>.fbx` maps slots to `Assets/Art/M_<slot>.mat`; a temporary editor method (named for what it does, deleted before the commit) makes `PF_<Name>` and places it; a case in `BuiltModelTests` (EditMode).
3. **Palette** in `SceneCapture` shots: a neutral patch in both pictures (white, grey, or a grey metal), `measure.py measure SPEC shot.png`, anchor one colour, derive the rest from the ratios, `measure.py correct`, set with `MaterialTuning.SetFromCommandLine`. Stop within a few hundredths.
   No shared neutral? `SceneCapture -captureGreyCard x,y,z` against something grey in the photograph gives hue only (compare chromaticity), not value. Match a median over the whole face, not a corner. Flat colour cannot carry grain: report the spread, do not chase it.
4. **Budget**: `build.py` prints triangles and slots; check against `docs/performance-budget.md`.

Unity commands run in batch mode as `docs/headless-unity.md` says; `Tools/gates.sh all` before each commit.

## How it ends

The list reads correctly in the Unity capture, or a line cannot and you say which and why; either way the findings go in the plan, and anything general into `docs/building_assets.md`.
Source files never name plans or phases.
