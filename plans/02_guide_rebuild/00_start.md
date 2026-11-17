---
status: in progress
priority: 0
depends_on: [01_repo_split]
description: |
  Rebuild this repo into the Unity guide now that the naval game lives in artificial-pirate:
  replace the ship slice with a small neutral sample over the generic runtime, and turn the docs
  into setup from zero, a bootstrap how-to, a pattern index, the AI workflow and distribution.
---

# Rebuild this repo into the Unity guide

Draft spin-off, raised 2026-11-14 with [`01_repo_split`](../01_repo_split/00_start.md); brainstorm and questions written 2026-11-17, after that folder closed. Q1 to Q10 answered 2026-11-17, all as recommended; phases in [`tracking.md`](tracking.md).

## Where this came from

In the flutter split the guide clean-up started as the last phase of the split and became its own folder (`flutter-setup-project` `27_guide_rebuild`, still a draft there); here it was a separate folder from the start.
What the guide is was settled by `01_repo_split` Q1: "a neat bootstrap, plus links to the projects that implement a pattern worth copying", not a gallery of samples.

## Settled already

From `01_repo_split`, done 2026-11-17:

- The game lives in [artificial-pirate](https://github.com/Pitrified/artificial-pirate), bootstrapped from `8b521a7` here; its `plans/01_bootstrap/` is the worked example of a project started from this repo.
- Template leftovers gone, MCP pinned, guide ids set (`com.pitrified.unitysetup`, "Unity Setup"), save system generic (`SaveEnvelope<T>`, the game's section in `Game.Gameplay`), debug overlay in `Persistent.unity`.
- The AI workflow docs rewritten for Claude Code running its own gates in batch mode (`docs/headless-unity.md`, the playbook, one MCP doc).
- History stays here: `docs/tracking.md` phases 0 to 8 and `scratch/`.
- artificial-pirate links four of this repo's docs on `main`: `docs/ai-development-playbook.md` (and its `#3-the-work-loop`), `docs/mcp-setup.md` (and `#2-setup`), `docs/dev-environment-setup.md`, and the repo root. A rename here breaks them, so either the paths stay or that repo's links change in the same phase.

## Current state, read 2026-11-17

- **The ship slice is still here**: `Scripts/Gameplay/` (ship controller and state, ship and camera tuning, chase camera, `GameSaveData` with the ship's section), `PF_Ship`, `PF_Island`, `M_Water`, `M_Island`, `SG_Water.shader`, the two tuning assets, `Game.unity`, `ShipControllerTests`, `CameraControllerTests`, and `docs/systems/ship-controller.md`, `camera-controller.md`.
- **The generic runtime knows the ship**: `GameManager` holds `_activeShip`, has `RegisterShip(ShipController)` and writes the ship's state on save; `GameSceneWiring` injects input into the ship and points the camera at it. `GameManagerTests` (13 mentions), `PauseResumeTests` (8) and `SaveRestoreTests` (16) drive the ship.
- **Game docs**: `docs/functional-specs.md` is the game's spec (ship, sea, water, chase camera); `performance-budget.md`, `build-and-release.md`, `google-play-private-alpha.md` are written for this game but are mostly generic mobile Unity.
- **No root `README.md`**, and no doc on how a session starts a new project from this repo.
- **The project folder is `UnitySetupPrpj/`**, a typo-like name; artificial-pirate renamed its copy.
- **The pre-commit hook does not do its first job**: it blocks `^(Library|Build|...)/`, anchored at the repo root, but the Unity project is in a subfolder, so `UnitySetupPrpj/Library/` never matches (checked: `grep -cE` gives 0). `.gitignore` catches those folders anyway, so it has not mattered. Its `for f in $STAGED` loops also split on the space in `UI Toolkit/`. artificial-pirate has the same hook.
- **`docs/copilot-instructions-template.md`** is a Copilot-era template (48 lines); the real instructions file, `CLAUDE.md` and `AGENTS.md` are now the pattern a new project copies.
- **The gates are commands in a doc**, typed out by each session; `scratch` wrappers did the work in the split.

## Open questions

- Q1: the sample. What does the guide's Unity project run once the ship goes?
  - a. a neutral placeholder: a capsule on a plane, moved by the same input actions, followed by the chase camera, its position saved and restored. It exercises boot, scenes, input, save, pause, UI, audio and logging, and `GameManager` stops knowing about ships (it registers a generic "saveable" or the avatar, decided in the phase). Ship, water and tuning patterns are linked to artificial-pirate.
  - b. keep the ship slice as the sample. No work, but it is a second copy of the game that drifts from artificial-pirate, and every new project deletes a ship.
  - c. no gameplay at all: Boot, Persistent and Menu, with an empty Game scene. Smallest, but save, pause and the PlayMode restore test then have nothing to act on.
  - Recommended: a. The runtime is the thing being demonstrated, and a placeholder avatar is the smallest thing that exercises all of it; the chase camera is generic enough to stay.
  - ANS: the recommendation, the user, 2026-11-17.
- Q2: the project folder name.
  - a. `UnitySetup/`, matching the product name.
  - b. keep `UnitySetupPrpj/`.
  - Recommended: a, done in the same way as artificial-pirate's (one `git mv`, the scripts and docs follow; the guide links artificial-pirate uses are under `docs/`, so they do not move).
  - ANS: the recommendation, the user, 2026-11-17.
- Q3: the guide's spec. `functional-specs.md` is the game's.
  - a. rewrite it as the sample's spec, short, keeping the shape a new project copies (overview, the locked-decisions table, lifecycle, scenes, save schema, definition of done).
  - b. delete it; a new project writes its own from nothing.
  - Recommended: a. The locked-decisions table and the "anything not listed is not decided" rule are the pattern; the sample needs a spec for its tests to point at.
  - ANS: the recommendation, the user, 2026-11-17.
- Q4: the docs written for the game but mostly generic: `performance-budget.md`, `build-and-release.md`, `google-play-private-alpha.md`.
  - Recommended: keep all three here, with the game's names and numbers made generic (the budget's numbers stay as the default for a mid-range Android target). artificial-pirate keeps its own copies.
  - ANS: the recommendation, the user, 2026-11-17.
- Q5: `docs/copilot-instructions-template.md`.
  - Recommended: delete it. The bootstrap how-to (Q6) says to copy `.github/copilot-instructions.md`, `CLAUDE.md` and `AGENTS.md` and change what names the project, as artificial-pirate did.
  - ANS: the recommendation, the user, 2026-11-17.
- Q6: a bootstrap how-to, `docs/bootstrap-new-project.md`: how a session starts a new Unity project from this repo (plan first, `git archive`, the renames with an editor method in batch mode, docs, the gates in a fresh clone, the human steps), with artificial-pirate's `plans/01_bootstrap/` as the worked example.
  - Recommended: yes; it is the guide's main reader, as in the flutter guide's brainstorm.
  - ANS: the recommendation, the user, 2026-11-17.
- Q7: the pattern index.
  - a. a section of the root README, one line per pattern with the link: artificial-pirate's ship controller with its tuning ScriptableObject, the water shader graph, and anything later projects add.
  - b. a separate `docs/patterns.md`.
  - Recommended: a, until it outgrows a screen.
  - ANS: the recommendation, the user, 2026-11-17.
- Q8: a gates script, `Tools/gates.sh [compile|edit|play|all]`, running the commands of `headless-unity.md` §3 and exiting non-zero on any `error CS`, `warning CS` or failed test; the doc then points at it.
  - a. yes, here and copied to artificial-pirate.
  - b. no, the doc's commands are enough.
  - Recommended: a. Every session so far wrapped the same commands in a scratch script; one real script is less to get wrong. CI stays out (it needs a Unity licence on a hosted runner).
  - ANS: the recommendation, the user, 2026-11-17.
- Q9: the pre-commit hook's two bugs (the anchored pattern, splitting on spaces).
  - Recommended: fix here in the template review, and the same fix in artificial-pirate as its own small change there.
  - ANS: the recommendation, the user, 2026-11-17.
- Q10: `docs/tracking.md` and `scratch/`.
  - a. stay where they are; the root README says they are the history of how the template and the game were built.
  - b. move to `docs/history/`.
  - Recommended: a. Moving breaks the diary's own links again and gains nothing.
  - ANS: the recommendation, the user, 2026-11-17.

## Phases

1. **Sample** (Q1): the ship slice out, the placeholder in, `GameManager` and `GameSceneWiring` generic, tests rewritten; gates and a capture.
2. **Folder rename** (Q2), with the scripts and docs following.
3. **Template review**: the hook fixed (Q9), `Tools/gates.sh` (Q8), `setup.sh` and `.gitattributes` read against what a new project needs.
4. **Docs**: root README with the pattern index (Q7) and the history note (Q10), the spec (Q3), the generic docs (Q4), the bootstrap how-to (Q6), the template deleted (Q5), `docs/README.md` and the instructions to match; link check, and the four paths artificial-pirate links still resolve.
5. **Verify** in a fresh clone on disk: gates, the `dev` APK, link and plan checks. HUMAN: the APK on the phone.

## Out of scope

- The asset method, [`03_asset_method`](../03_asset_method/00_start.md); its spike's barrel lands in this project whichever sample Q1 picks.
- Changes to artificial-pirate other than the hook fix (Q9) and any link that a rename here would break.
- CI.
