# Guide rebuild - implementation tracking

This repo turned into the Unity guide: a neutral sample over the generic runtime, a template reviewed for a new project, docs for setting one up. Brainstorm, state and answers in [`00_start.md`](00_start.md).

## Key decisions

From `00_start.md` Q1 to Q10, 2026-11-17, all as recommended:

- **The sample is a placeholder avatar** (Q1): a capsule on a plane, the chase camera, its position saved. Ship, water and tuning are linked to artificial-pirate.
- **Renamed by moving files, not rewriting them**: the ship's scripts and prefab become the avatar's with `git mv`, so `.meta` GUIDs and every scene reference survive.
- **The project folder becomes `UnitySetup/`** (Q2), and the game's names left here become the guide's.
- **One gates script** (Q8), `Tools/gates.sh`, here and in artificial-pirate.
- **History stays in place** (Q10).

## Phases

| #  | Phase                    | Plan                                     | Status  |
| -- | ------------------------ | ---------------------------------------- | ------- |
| 01 | Neutral sample           | [`01_sample.md`](01_sample.md)           | done    |
| 02 | Folder and names         | [`02_rename.md`](02_rename.md)           | done    |
| 03 | Template review          | [`03_template.md`](03_template.md)       | planned |
| 04 | Docs                     | [`04_docs.md`](04_docs.md)               | planned |
| 05 | Verify                   | [`05_verify.md`](05_verify.md)           | planned |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-14 : raised as a sibling of `01_repo_split`.
- 2026-11-17 : brainstorm and Q1 to Q10 written after `01_repo_split` closed; all answered as recommended. Five phases planned.
- 2026-11-17 : phase 01 - the ship slice became a capsule avatar on a ground plane, island and water assets removed, save schema v3. EditMode 114/114, PlayMode 4/4, 0 warnings.
- 2026-11-17 : phase 02 - `UnitySetup/`, `unity-setup` names. A moved project folder needs a fresh `Library/` (PlayMode failed 2/4 on the moved one). EditMode 114/114, PlayMode 4/4, `dev` APK `com.pitrified.unitysetup`.
