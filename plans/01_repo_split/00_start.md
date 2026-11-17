---
status: draft
priority: 1
description: |
  Split artificial-pi, the naval game, out of this repo into its own repo, artificial-pirate,
  written by a Claude session from a plan and the audit. This repo becomes the Unity guide:
  setup from zero, the generic runtime systems, the AI and MCP workflow, build and distribution.
  The same split flutter-setup-project made with fala-language-tutor, with its lessons applied.
---

# Split the naval game from the Unity template

Raised 2026-11-14 as a draft. No phases agreed yet; the proposed phases are in [`tracking.md`](tracking.md).

## Where this came from

The ask: "We want to do for this template for unity the same we did for flutter. Create a plan in unity template, new project is artificial pirate the naval game, done well."

"The same we did for flutter" is `flutter-setup-project`'s `plans/20_repo_split/`:
an audit gave every tracked file a destination, a clean-up removed dead code and couplings before anything was duplicated,
a session wrote `fala-language-tutor` from a plan placed in that new repo as its `plans/01_bootstrap/`,
and the clean-up of the source into a guide was split off as a feature of its own (`27_guide_rebuild`).
This plan follows that shape and folds in what that run found.

## Why the repo is like this

The repo is named `unity-setup-project`, its Unity project folder is `UnitySetupPrpj/`, and every doc calls the project `artificial-pi`.
It started as a setup exercise (phases 0 to 3 of `docs/tracking.md` are toolchain, repo scaffolding and project creation) and a game grew inside it.
Both halves are real:

- **Template and patterns**: the docs set (AI playbook, coding standards, project structure, git workflow with LFS, testing strategy, performance budget, MCP setup and best practices, dev environment, build and release, Play private alpha),
  the repo scaffolding (`.gitattributes`, `.editorconfig`, `Tools/setup.sh`, the pre-commit hook),
  and the generic runtime: Boot / Persistent / Menu / Game scenes, `GameBootstrap`, `GameManager`, `SceneLoader`, `InputManager`, `AudioManager`, `UISystem`, `SaveSystem`, `Log`, the build pipeline and `Tools/build-android.sh`.
- **Product**: the ship and its tuning, the chase camera, the water shader, the island, the save schema with the ship in it, the functional spec, and the open roadmap phases 9 to 13 (MCP, performance, upload key, Play Console, demo-ready).

## Current state, read 2026-11-14

- `main` is at `22cb365` (2026-05-05), "finish track 5 start 6".
- `origin/feat/ph6` is 8 commits ahead at `2c0c76b` (2026-05-19), `main` is its ancestor. It carries phase 6 (ship, camera, water, island, overlay, save wiring), phase 7 (three PlayMode tests) and most of phase 8 (build pipeline, `Tools/build-android.sh`, `build.env.example`), with "update after installing apk once" as its last commit.
  Its `docs/tracking.md` still has the three HUMAN items of phase 8 unchecked.
- 297 tracked files on `feat/ph6`, most of them `.meta` and `ProjectSettings/`.
  One LFS file (`Assets/TutorialInfo/Icons/URP.png`).
- Unity `6000.4.4f1`. `applicationIdentifier` is still the Unity template default (`com.UnityTechnologies.com.unity.template.urpblank`), `productName` is `UnitySetupPrpj`, `companyName` is `DefaultCompany`. Nothing has been uploaded to Play, so the id is still free to choose.
- `Packages/manifest.json` takes the MCP package from `CoplayDev/unity-mcp.git#main`, not pinned, which `docs/mcp-setup.md` and the hard rules ask for.
- The plans live in `docs/tracking.md` (a checklist by phase) and `scratch/scratch-pad-phNN.md` (per-phase working notes). There is no `plans/` folder before this one.
- This box had no Unity Editor when this was read.
  Since 2026-11-17 it has Unity 6000.4.4f1 with the Android module, licensed, run headless (box note `~/repos/plans/2026-11-17_00_unity_editor_headless.md`).
  A session here compiles the project, runs the EditMode and PlayMode tests and builds the `dev` APK in batch mode; rendering needs `xvfb-run ... -batchmode -force-vulkan`.
  What stays with a person: installing and playing the APK on a device, and anything judged by eye in the Editor.
- Two repo bugs found by the first run here, fixed on this branch 2026-11-17:
  `Tools/build-android.sh` never auto-detected the editor (it wrote NUL separators with `echo -n "$p"$'\0'`, which a bash string cannot hold, so it only worked with `UNITY_PATH` set), and `GameBootstrap.cs` raised CS4014 on the fire-and-forget `UnloadSceneAsync`, now an explicit discard.

## Couplings found while reading

- `Game.Systems` is the leaf assembly, generic by its own rules, but `Systems/Save/SaveData.cs` defines `ShipSaveData` and `SaveData.ship`. The save system's schema is the game's.
- `Game.Core` references `Game.Gameplay` by design (it is the composition root, `docs/project-structure.md` §3), so `GameManager.RegisterShip(ShipController)` and `GameSceneWiring` naming the ship and camera are not layering violations.
  They do mean the guide's `GameManager` cannot keep its current shape without a ship.
- `Tools/build-android.sh` hard-codes `UnitySetupPrpj` and `~/.config/artificial-pi/build.env`.

## Lessons from the flutter split, applied here

- **Audit first, with a coverage check.** Every tracked path matched by a pattern in the audit table, checked by a script, seen failing first. In flutter the audit still misfiled one class (`AppController`); the new repo's plan should expect corrections.
- **Clean before duplicating.** Dead code, template leftovers and couplings are fixed here, once, before the copy.
- **The new repo is written from a plan that lives in it**, `plans/01_bootstrap/`, numbered from 01.
- **The guide clean-up is its own feature.** In flutter it started as phase 05 and was superseded by a separate folder. Here it is a draft folder from the start, [`02_guide_rebuild`](../02_guide_rebuild/00_start.md), and this folder ends when the game runs from its own repo.
- **A fresh-session note** in the new repo saying what to install. Here the note says how a headless session runs Unity (batch mode, Xvfb and Vulkan for rendering) and what stays with a person.

## What is different from flutter

- **Assets cannot be rewritten from scratch.** Scenes, prefabs, materials and asmdefs reference each other by the GUIDs in `.meta` files. "Written, not extracted" can only apply to code and docs; the Unity project folder is copied with its `.meta` files intact.
- **Gates in batch mode on the box, no CI.** Flutter had `scripts/check.sh` and CI.
  Here a session on the box closes a phase that changes Unity files with: the project compiles with 0 errors and 0 warnings, EditMode and PlayMode tests green, and where it matters the `dev` APK builds.
  The device run stays a HUMAN step. (Until 2026-11-17 every gate here was human; the box had no Unity.)
- **LFS.** The copy has to carry LFS objects, not pointer files. The new repo needs `git lfs install` and its own `.gitattributes`, and the push uploads the objects.
- **The plans convention is new to this repo.** Phase 01 adopts `plans/` here; the old `docs/tracking.md` and `scratch/` stay as history until the guide rebuild decides their place.

## Proposed approach

1. Land `feat/ph6` on `main`, so the split starts from the latest work. Adopt `plans/`.
2. Audit every tracked path: `guide`, `pirate`, `both`, `drop`. Coverage checked by a script.
3. Clean up here before duplicating: template leftovers, the save schema coupling, the MCP pin, the project naming.
4. Write `artificial-pirate`: plan first in the new repo, then the copy, the renames, docs, open roadmap phases moved; verified in the Editor and on the device by the user.

The guide rebuild follows in `02_guide_rebuild`.

## Cheaper alternatives, ruled out unless reopened

1. Rename this repo to `artificial-pirate` and extract the template later, when a second Unity project wants it.
   It is the honest choice if there is no second Unity project in sight (Q1).
2. Keep one repo and mark the generic systems in place.

## Open questions

- Q1: is there a second Unity project that will start from the guide, or is the split for tidiness?
  The flutter split's Q1 answer was that the guide is what a session reads to bootstrap a new app, and fala-language-tutor was the first app built that way. If the same holds here, artificial-pirate is the first Unity one.
  - a. same as flutter: the guide is the bootstrap source for future Unity projects, artificial-pirate is the first.
  - b. tidiness only: take cheaper alternative 1 instead.
  - Recommended: a, if a second Unity project is plausible within a year; otherwise b costs a rename and an afternoon.
  - NEW_ANS:
- Q2: the new repo's name and the project's name.
  The docs say `artificial-pi`; the ask says "artificial pirate".
  - a. repo `artificial-pirate`, product name `Artificial Pirate`, docs renamed from `artificial-pi`.
  - b. repo `artificial-pi`, names unchanged.
  - Recommended: a, the full name reads as what it is and the rename is a text substitution done once during the copy.
  - NEW_ANS:
- Q3: who creates the GitHub repo, and when?
  This box has no GitHub credentials, and pushes go through your `ssh -A` session from g4.
  - Recommended: you create `Pitrified/artificial-pirate` empty (no README, no `.gitignore`) before phase 04, and push it from g4; LFS needs to be enabled on it, which is the default for GitHub.
  - NEW_ANS:
- Q4: where does the split start from? `feat/ph6` holds phases 6 to 8 and is not merged.
  - a. you merge `feat/ph6` into `main` first (it fast-forwards), and everything after branches from `main`.
  - b. this folder's branch is cut from `feat/ph6`, which stays unmerged.
  - Recommended: a. `main` is an ancestor, so it is a fast-forward with no conflicts, and the template's history then reads in order.
  - NEW_ANS:
- Q5: the three open HUMAN items of phase 8 (debug keystore gitignored, `dev` APK builds, logcat clean).
  The last commit says an APK was installed once.
  - a. confirmed done: tick them in `docs/tracking.md` during phase 01.
  - b. not all done: they move to the new repo as its first verification.
  - Recommended: tell me which; if in doubt, b, since phase 04 builds an APK anyway.
  - NEW_ANS:
- Q6: git history for the new repo.
  - a. none: copy the tracked files of one commit with `git archive`, as fala-language-tutor did. `.meta` GUIDs survive because they are file content.
  - b. `git filter-repo` on the game's paths, keeping its commits.
  - Recommended: a. The commits that built the game also built the template and live on here; the bootstrap plan names the source commit.
  - NEW_ANS:
- Q7: the Unity project folder name in the new repo, today `UnitySetupPrpj/`.
  - a. rename to `ArtificialPirate/` during the copy, with `Tools/build-android.sh`, `.vscode/` and docs following. Unity regenerates `Library/` on first open; GUIDs do not change.
  - b. keep `UnitySetupPrpj/` and rename later.
  - c. move the project to the repo root.
  - Recommended: a. The typo-like name would follow the game forever, and the first open in the new repo regenerates `Library/` anyway. c changes how `.gitignore` and `Tools/` work and gains little.
  - NEW_ANS:
- Q8: the Android `applicationIdentifier`. It is permanent once a build is on Play, and today it is the Unity template's.
  - a. game: `com.pitrified.artificialpirate`; guide: `com.pitrified.unitysetup`, set in phase 03.
  - b. another prefix you already use (fala used `com.fala.app`).
  - Recommended: a, set here in phase 03 for both so neither repo ever builds with the template id; the game's id is then changed once more during the copy.
  - NEW_ANS:
- Q9: the save schema coupling. `SaveData` in the generic `Game.Systems` holds `ShipSaveData`.
  - a. fix here before duplicating: the save system stores a generic payload (versioned JSON with a game-owned section type), the ship section lives in `Game.Gameplay`. Both repos start from the clean shape.
  - b. leave it; the new repo keeps it as is and the guide fixes its own copy in `02_guide_rebuild`.
  - Recommended: b, against the flutter precedent. The fix touches the save system, its tests, `GameManager` and the PlayMode restore test, and none of it can be compiled here; doing it in the game repo first and lifting it to the guide later is one Unity session instead of two. Choose a if you will be at the workstation for phase 03 anyway.
  - Update 2026-11-17: the box now compiles and runs the tests, so the reason for b (no compile here, one Unity session instead of two) no longer holds. Recommended now: a, the flutter precedent, both repos start from the clean shape.
  - NEW_ANS:
- Q10: Unity template leftovers: `Assets/TutorialInfo/`, `Assets/Readme.asset`, `Assets/Scenes/SampleScene.unity`, `Assets/Settings/SampleSceneProfile.asset`.
  - Recommended: drop them in phase 03 if the audit finds no reference to their GUIDs; this also removes the only LFS file.
  - NEW_ANS:
- Q11: the MCP package is taken from `#main` of `CoplayDev/unity-mcp`.
  - Recommended: pin it to the commit in `packages-lock.json` in phase 03, so both repos start reproducible.
  - NEW_ANS:
- Q12: where do the old diary files go? `docs/tracking.md` phases 0 to 8 and `scratch/` record how both halves were built; phases 9 to 13 are the game's open roadmap.
  - a. history stays here; phases 9 to 13 become plan folders in the new repo (`02_mcp`, `03_performance`, ... or merged where small), each linking back to the source here.
  - b. copy `scratch/` to both.
  - Recommended: a, matching the flutter answer that done folders stay in the source as its history.
  - NEW_ANS:
- Q13: who does phase 04, and where?
  - a. the file work (copy, renames, docs, plans) in a session on this box; you push from g4, open the project on the workstation, run the Test Runner and build the APK.
  - b. the whole phase in a session on the workstation, with Unity MCP, so the session can read the console and run tests itself.
  - Recommended: a for the copy and text, then a short session on the workstation for any compile fix. The rename is mechanical; the Unity checks are yours either way.
  - Update 2026-11-17: with Unity on the box, a covers the compile, the tests and the `dev` APK too; what is left to you is the push from g4 and the device run. Recommended now: a, with those two as the only HUMAN steps.
  - NEW_ANS:
