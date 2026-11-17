---
status: planned
---

# Phase 02 - Match the barrel to a photograph

## Overview

The method on a real reference: photographs found online under a licence that allows keeping them in a public repo, one chosen, a "what right looks like" list written from it, then passes until the list reads correctly or a pass says it cannot.

## Plan

- Search for photographs of a wooden barrel seen from the side, CC0, public domain or CC-BY (Wikimedia Commons first). Keep two or three candidates, pick one; record source, author, licence in `UnitySetup/Assets/Art/References/CREDITS.md` (references are not shipped in a build, but they are in the repo).
- Write the list before the first render: silhouette (height to bilge ratio, head to bilge ratio), stave count and width as seen, hoop count, positions and width, wood and hoop hue and value.
- Passes, one class of change each, numbered `r<N>` in a scratch folder: shape in a Blender probe beside the photograph, measured; palette in the Unity capture, measured against the photograph's patches as ratios to a neutral reference.
- Budget as a stop condition: triangles and materials against `docs/performance-budget.md`.

## Done when

- Every line of the list reads correctly in the Unity capture, or the run says which cannot and why; the findings are written.

## What the implementation found
