---
status: done
---

# Phase 02 - Match the barrel to a photograph

## Overview

The method on a real reference: photographs found online under a licence that allows keeping them in a public repo, one chosen, a "what right looks like" list written from it, then passes until the list reads correctly or a pass says it cannot.

## Plan

- Search for photographs of a wooden barrel seen from the side, CC0, public domain or CC-BY (Wikimedia Commons first). Keep two or three candidates, pick one; record source, author, licence in a credits file (references are not shipped in a build, but they are in the repo).
- Write the list before the first render: silhouette (height to bilge ratio, head to bilge ratio), stave count and width as seen, hoop count, positions and width, wood and hoop hue and value.
- Passes, one class of change each, numbered `r<N>` in a scratch folder: shape in a Blender probe beside the photograph, measured; palette in the Unity capture, measured against the photograph's patches as ratios to a neutral reference.
- Budget as a stop condition: triangles and materials against `docs/performance-budget.md`.

## Done when

- Every line of the list reads correctly in the Unity capture, or the run says which cannot and why; the findings are written.

## What the implementation found

- 2026-11-18. **Search**: the Commons API by category (`Wooden barrels`, `Barrels`, `Wine barrels`) with each file's licence in one call; about 200 files, filtered to CC0, public domain and CC BY. Wikimedia answered 429 after four quick downloads and again on a full-size original: fetch one at a time with a pause, and work from the 960 px thumbnails.
- **Candidates** on a contact sheet: an Auckland Museum keg (CC BY 4.0, clean side view, no dimensions in its record), a public-domain cellar of barriques (Bernard Spragg), a CC0 wine cask, a gunpowder store. Picked the barriques: public domain, and the same kind of barrel as the builder's real numbers. Kept in `Tools/asset_builders/references/barrel/` with `references/CREDITS.md`, outside `Assets/` so Unity does not import it.
- **What right looks like**, written from it before the first pass: (1) a belly of 0.72 m at the bilge on 0.56 m heads and 0.95 m length (the barrique's numbers; the photo is in perspective); (2) six flat galvanised bands, three at each end, the middle half bare; (3) bands about 6 % of the length wide; (4) narrow staves, seams faint; (5) a cork bung at the bilge; (6) pale oak against light grey steel.
- **Measured, not eyeballed**: a chroma profile along the front barrel found the bands (low chroma) at 3, 11 and 30 % from each end, averaging both ends to cancel the perspective. The blind draft had six bands evenly spread at 5, 20 and 36 %.
- **Pass 1, shape**: bands moved, made flat (sharp ring edges; smooth shading had made them read as round tubes), 28 staves, a cork as a third slot. **Pass 2**: band width 0.04 to 0.055 m from a side-by-side with the reference rotated upright.
- **Palette** in the Unity capture. A cellar has no white, so the galvanised hoop is the neutral in both pictures. The ratio fixes the colours' relation only; the wood was anchored on the reference's lit hue at a pale-oak value, the hoop derived from the ratio, then corrected once in linear: wood/hoop off by +0.18 / +0.12 / +0.06 after the first try, +0.002 / -0.005 / +0.003 after the correction. Two captures.
- New permanent tool `MaterialTuning`, so palette passes run headless; `measure.py` (patches as ratios to a neutral, spread, chroma profile, linear correction, crops) adapted from house-renderer's.
- **Not matched, on purpose**: the grain. Luminance spread is 30 in the photograph and 8 in the render: flat colours cannot carry it. The upgrade path, if wanted, is a small tiling texture or per-stave tones through a palette texture.
- `BarrelModelTests` failed after the bung went in: the pivot check used the bounds' centre, which the cork moves by 7 mm. It now checks the centre of the base ring, which is what "pivot at the base centre" means.
- Budget: 1978 triangles, three materials (three draw calls a barrel, nine for the scene's three, of 80).
- Gates: compile 0 warnings, EditMode 118/118, PlayMode 4/4.
