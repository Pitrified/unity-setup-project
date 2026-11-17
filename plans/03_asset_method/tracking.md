# Asset method - implementation tracking

Building game objects from a reference photograph: a Blender builder script exports FBX into Unity, passes are compared against the photograph until it reads the same. Brainstorm, decisions and answers in [`00_start.md`](00_start.md).

## Key decisions

From `00_start.md` Q1 to Q6:

- **Blender `bpy` from PyPI in a `uv` project in the repo** (Q2), `Tools/asset_builders/`; Blender materials do not cross into Unity, URP materials are made there.
- **The skill and the method doc live in this guide** (Q1); a project copies them and keeps its own findings.
- **A barrel first** (Q4), in this project's sample scene, where the water folder can float it later.
- **Everything headless on the box** (Q6): Cycles on the GPU for the probe, batch-mode Unity for import, test and capture.

## Phases

| #  | Phase                        | Plan                                         | Status  |
| -- | ---------------------------- | -------------------------------------------- | ------- |
| 01 | Pipeline spike               | [`01_spike.md`](01_spike.md)                 | done    |
| 02 | Match the barrel to a photo  | [`02_barrel.md`](02_barrel.md)               | done    |
| 03 | Method doc and skill         | [`03_method.md`](03_method.md)               | planned |
| 04 | Verify                       | [`04_verify.md`](04_verify.md)               | planned |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-14 : drafted with Q1 to Q5 answered; Q6 (Unity on the box) answered b.
- 2026-11-17 : Unity installed headless on the box; the condition on Q1 to Q5 met.
- 2026-11-18 : picked up unsupervised on the user's go, after `02_guide_rebuild` closed. Four phases planned; branch `feat/03_asset_method` from `feat/02_guide_rebuild`.
- 2026-11-18 : phase 01 - builder to FBX to Unity headless: `Tools/asset_builders`, deterministic FBX, `ModelSetup` and `SceneCapture` editor tools, `BarrelModelTests`, three barrels in the sample scene. Gates green.
- 2026-11-18 : phase 02 - public-domain barrique photograph chosen from Commons; hoop layout measured from a chroma profile; two shape passes and two palette captures, wood-to-hoop within 0.01. Grain left unmatched (no texture). `MaterialTuning` and `measure.py` added.
