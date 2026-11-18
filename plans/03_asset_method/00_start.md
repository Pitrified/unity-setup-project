---
status: done
priority: 0
description: |
  A method, and a skill, for building good-looking game objects from a real-world photograph:
  a Blender builder script exports FBX into Unity, and passes are compared against the
  reference until it reads the same. The general idea of house-renderer's unattended
  render loop, adapted to a stylized mobile game rather than ported.
comment: |
  Generic, so it lives in the guide; the first real use is the ship in artificial-pirate.
---

# Building assets from a reference

Raised 2026-11-14 while planning [`01_repo_split`](../01_repo_split/00_start.md). Picked up 2026-11-18, unsupervised overnight, on the user's go: "Do asset method to polish the skills a bit. All unsupervised, pick a few pictures on the internet, select the references, iterate. Commit." Phases in [`tracking.md`](tracking.md).

## Where this came from

The ask, condensed: keep house-renderer's general method so we can build good-looking objects for the game.
Take a picture of a pirate ship, or whatever object or component is being built, in the real world, and iterate the model until it looks similar.
"We keep the general idea, not a 1 to 1 port."

The source is `house-renderer`:
the project skill `.claude/skills/match-reference/` (`SKILL.md` and `measure.py`),
`docs/building_assets.md` (the passes, the probe, "Running it unattended", "How a run ends"),
and plan briefs such as `plans/74_zucchetti/00_start.md` (a numbered "what right looks like" list written before the first render, then run overnight).

## Decided with the user, 2026-11-14

- **Models are made in Blender and imported as FBX**, picked as the industry standard.
  ProBuilder is for greyboxing levels and C# generated meshes are for content built at runtime; neither is how game art is usually made.
  Builders are `bpy` scripts, so they run headless on the box without Unity, as house-renderer's do.
- **Taken assets: any licence that allows shipping inside a commercial app with ads**, with attribution where the licence asks for it.
  - Yes: CC0 and public domain; CC-BY 4.0 (credit with author, title, link, licence, changes); MIT, BSD, Apache 2.0, zlib (keep the notice); the Unity Asset Store Standard EULA (shipped inside a built game only, the source files never in a public repo).
  - Avoid: CC-BY-SA, since share-alike's reach into a game is unclear.
  - No: CC-BY-NC (ads are commercial), CC-BY-ND (we have to adapt), "editorial" or "personal use" only, no stated licence.
  - Every taken asset gets a row in a credits record: source URL, author, licence, date, what was changed. A build that contains a CC-BY asset needs an in-app credits screen.
  - The locked spec says no ads in v0.2. This only makes sure no asset taken now closes that door; it does not decide ads.

## What is kept from house-renderer

- **"What right looks like" first.** Before the first render, a numbered list from the reference of what has to read correctly. It is the checklist and the stop condition an unwatched loop otherwise lacks.
- **Read how this kind of thing is modelled, then find real numbers** for the exact object. Scale off an object of standard size in the photograph, not off proportions.
- **Draft the whole object blind**, then a shape pass at cheap settings, then a detail pass.
- **A probe**: the object alone on a plain ground, framed like the photograph, written side by side with it.
- **Judge in the real conditions**: house-renderer renders the real room rather than a swatch; here the last word is a capture in the game's own scene, camera, light and water.
- **Scratch output**, numbered `r<N>`, never over a kept file.
- **Measure, do not eyeball**: patch medians as ratios to something white in the same picture, corrections in linear, spread for texture as well as mean, side-by-side crops for the look.
- **The script is the source, the asset its output**: a builder regenerates the model, a taken asset included, so the binary in the tree is reproducible.
- **A test that the model is the size its sheet says**: from the exported mesh's bounds.
- **Parametric where the object is, fixed where it is not.**
- **One class of change per pass; a fix that does not work is information; look at the reference again every pass.**
- **Build it when the numbers exist, take it when the shape is the thing** (people, animals, foliage), within the licences above.
- **How a run ends**: objectives met, or clearly impossible and it says so, and in both cases a report that hands the next move back. Time given is a budget, not a timeout.
- **No watchers**: a background job's exit notification is the signal.
- **A findings doc that grows**: each pass that finds something adds a line.

## What is added for Unity and mobile

- **The budget is a stop condition**: triangles, materials, texture size and draw calls against `docs/performance-budget.md`. A pretty ship over budget has not met its objectives.
- **Axes, fixed once in the doc before the first builder**: Unity is Y up, +Z forward, left-handed; the FBX export applies transforms with the forward axis set for Unity.
- **Materials are made in Unity**, as URP materials. FBX carries mesh, UVs and vertex colours or texture maps; Blender materials do not survive it reliably.
- **Two places to look**: shape passes on a Blender probe render on the box, where they are fast; the final look and the budget in a Unity capture on the workstation, since the player sees mobile URP shading.
- **What makes a game object**: colliders, LOD where it pays, a prefab whose pivot suits what drives it.
- **FBX goes in LFS**; builder and FBX are both committed.
- **The target is stylized**: match proportion, silhouette and palette, the same hue and value in our shading rather than the photograph's light.

## What is dropped

- The house domain: boxes as the truth, catalogue and palette files, roles, rooms, collisions, `interest.yaml`.
- The picture bookkeeping: sidecars, stale hashes, the bucket, `records`, ratings and `nice_images/`. A game has a few hero objects, not hundreds of tracked pictures; a small version comes back only if it hurts.
- Photoreal matching as the goal.
- The box and pretty looks and the light presets: one probe light in Blender, the game scene in Unity.
- house-renderer's cost tables and timings; this project measures its own.
- Blender pitfalls are not copied in bulk: each is added to the findings doc when it bites here.

## Headless

The user's condition on accepting Q1 to Q5 (2026-11-14): the whole method runs headless, with no person at a GUI inside the loop.
A search on 2026-11-14 gave the expected answers; the install on 2026-11-17 tried them on the box (box note `~/repos/plans/2026-11-17_00_unity_editor_headless.md`).
The condition is met.

- **Blender**: `bpy` from PyPI is a Python module with no window. Builders, the probe render, the FBX export and the measuring all run on the box. Settled by construction; the spike runs the first builder.
- **Unity import, tests and checks**: verified. `-batchmode -nographics -runTests -testPlatform EditMode` ran 114 of 114 tests with no display. The axis and scale check is an EditMode test on the imported mesh's bounds, not a look in the Editor.
- **Unity capture**: verified with a scratch editor method that renders a cube through a camera into a PNG.
  - With no X display the editor does not start at all, even in batch mode (`Error opening default X display`), so Xvfb is needed for any run that renders.
  - `xvfb-run -a -s "-screen 0 1280x1024x24" Unity -batchmode ...` renders, but on Mesa `llvmpipe` (OpenGL Core on the CPU).
  - Adding `-force-vulkan` renders on the Quadro RTX 3000 (NVIDIA 595.99.02). **The capture command is `xvfb-run ... Unity -batchmode -force-vulkan -executeMethod ...`**, without `-nographics`. PlayMode tests run the same way, 3 of 3.
- **The licence**: activated once from the terminal, with no GUI: the user ran `unity auth login` and `unity license activate --personal --accept-eula` (Unity CLI 1.0.1). The 2026-11-14 finding that Personal needs the Hub is out of date. Batch runs log `[Licensing::Module] Error: Access token is unavailable` and work regardless.
- **Where Unity runs**: on this box (Q6), installed 2026-11-17: Unity 6000.4.4f1 at `~/Unity/Hub/Editor/6000.4.4f1/`, with the Android module.

## Proposed shape, for when it is picked up

0. **Prerequisite, outside this folder**: Unity and Xvfb on the box, by a box-level plan note (Q6). Done 2026-11-17; a headless capture already works (see "Headless").
1. **Pipeline spike, headless end to end**: one trivial builder (a barrel) on the box, FBX into the guide's Unity project, a batch-mode import and an EditMode test of its axes and scale, and one capture of the barrel in the game scene under `xvfb-run ... -force-vulkan` through the editor method. Proves the round trip before any method is written down.
2. **Method doc and skill**: `docs/building_assets.md` in the guide, the skill and its measure script, the credits record format.
3. **First real object**, proving the loop end to end against a photograph.

## Open questions

- Q1: where do the skill and the method doc live?
  - a. the guide, `.claude/skills/<name>/` and `docs/building_assets.md`; a new project copies them at bootstrap, artificial-pirate first, and keeps its own findings.
  - b. a user-level skill in the dotfiles, the doc in each project.
  - Recommended: a, as `match-reference` is a project skill in house-renderer; the method depends on a project's pipeline and budgets.
  - ANS: a, the recommendation, the user, 2026-11-14, on condition that the whole method runs headless.
- Q2: how does the box get Blender for the builders?
  - a. `bpy` from PyPI in a small `uv` project inside the repo (for example `Tools/asset_builders/`), as house-renderer does with `bpy>=5.2.1`.
  - b. a Blender install, run as `blender -b --python`.
  - Recommended: a, no system install, pinned in a lock file, and the same setup as house-renderer.
  - ANS: a, the recommendation, the user, 2026-11-14. Headless by construction.
- Q3: how is the Unity capture taken on the workstation?
  - a. one editor method, `Game.Editor` side, that frames the prefab in the game scene and writes a PNG; called by `Unity -batchmode -executeMethod` or through MCP.
  - b. through MCP only.
  - Recommended: a, one capture code path with two ways to call it; batch mode does not depend on an open Editor.
  - ANS: a, the recommendation, the user, 2026-11-14, if headless. It is, with conditions: see "Headless" above and Q6.
- Q4: what proves the method first?
  - a. a small prop in the guide (a barrel or a crate), then the ship in artificial-pirate.
  - b. straight to the ship in artificial-pirate.
  - Recommended: a, a barrel finds the pipeline's problems for a fraction of a ship's passes.
  - ANS: a, the recommendation, the user, 2026-11-14.
- Q5: picked up when?
  - a. after `01_repo_split`, so the method doc and skill land in the guide and artificial-pirate copies them at its own pace.
  - b. now, in parallel with the split.
  - Recommended: a; the spike needs the guide's Unity project, which `01_repo_split` and `02_guide_rebuild` are about to reshape.
  - ANS: a, the recommendation, the user, 2026-11-14.

### Second batch (2026-11-14), from the headless condition

- Q6: where does the Unity half (import, tests, capture) run headless?
  The Blender half runs on the box either way.
  - a. on the workstation that already has Unity 6000.4.4f1 and an activated licence: a Claude session there runs `xvfb-run` batch mode, or the box reaches it over SSH when you allow it. The box builds the FBX; the workstation imports and captures; the PNG comes back for measuring.
  - b. install Unity on this box: system-level work with a plan note and your approval, over 10 GB with the Android module, and the Personal licence activated once in Unity Hub here, which needs a GUI once (X forwarding or a VNC session) and puts an account-bound licence file on a box meant to hold no such credentials.
  - Recommended: a. It needs no new install, keeps the licence where it already is, and the workstation is where the device builds happen anyway. b is the choice if the loop should run with the workstation off; it then needs its own plan note under `~/repos/plans/`.
  - ANS: b, the user, 2026-11-14, overriding the recommendation: Unity on this box, so the whole loop runs here.
    What that brings, read on the box the same day:
    - The install is system-level work: a box-level plan note under `~/repos/plans/` and the user's approval before anything is installed or any command handed over. It is a prerequisite of the spike, not part of this folder.
    - The box has 795 GB free, 23 GiB of memory available, and a Quadro RTX 3000 with 6 GB on driver 595.99. Xvfb is not installed and goes in the same note.
    - The box runs Ubuntu 26.04. Unity's supported Linux list for the 6000.x editors has named older Ubuntu LTS releases, so the editor may run unsupported here. The note checks the current list, and the spike's first result says whether it runs.
    - The editor version is the project's, `6000.4.4f1`, with the Android module (SDK, NDK, OpenJDK) so device builds run here too: the user, 2026-11-17.
    - The Personal licence is activated once in Unity Hub on this box, which needs a GUI once (X forwarding over the SSH session, or a temporary VNC). The resulting licence file is tied to the user's Unity account and lives on a box otherwise kept free of such credentials: the user's call. The note says where the file is and how to revoke it (return the licence from the Hub, or remove the activation from the Unity account).
    - Done 2026-11-17, and simpler than the above: no Hub and no GUI, the licence was activated from the terminal with the Unity CLI. Results in "Headless".
