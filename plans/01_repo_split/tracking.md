# Repo split - implementation tracking

artificial-pi, the naval game, leaves for its own repo, `artificial-pirate`, written by a Claude session from a plan and the audit.
This repo then becomes the Unity guide, in [`02_guide_rebuild`](../02_guide_rebuild/00_start.md).
Brainstorm, current state, decisions and open questions are in [`00_start.md`](00_start.md).

## Key decisions

From the answers in `00_start.md` (Q1 to Q15, 2026-11-17):

- **Audit first**, with a script that checks every tracked path is covered.
- **Clean before duplicating.**
- **The Unity project is copied, not rewritten**: `.meta` GUIDs carry every reference between assets.
- **The guide rebuild is its own folder**, `02_guide_rebuild`; this folder ends when the game runs from its own repo.
- **Building assets from a reference photograph is its own folder**, [`03_asset_method`](../03_asset_method/00_start.md). artificial-pirate gets the method when that folder lands; its pirate-specific findings stay in that repo.
- **Unity checks run in batch mode on the box; the device run is a human step.** Each phase that changes Unity files closes on a compile with 0 warnings and green EditMode and PlayMode tests, run here; the APK on a device is the user's. (Was "all human" until Unity was installed on the box, 2026-11-17.)

## Phases

| #  | Phase                              | Plan                                                 | Status |
| -- | ---------------------------------- | ---------------------------------------------------- | ------ |
| 01 | Land phase 6 work, adopt `plans/`  | [`01_land_ph6.md`](01_land_ph6.md)                   | done   |
| 02 | Audit                              | [`02_audit.md`](02_audit.md)                         | done   |
| 03 | Clean up before duplicating        | [`03_pre_split_cleanup.md`](03_pre_split_cleanup.md) | done   |
| 04 | Review the docs and AI workflow    | [`04_docs_review.md`](04_docs_review.md)             | done   |
| 05 | Write artificial-pirate            | [`05_pirate_repo.md`](05_pirate_repo.md)             | draft  |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-14 : bootstrapped from the flutter split (`flutter-setup-project` `plans/20_repo_split/`, `27_guide_rebuild`, and `fala-language-tutor` `plans/01_bootstrap/`). Read `main` and `origin/feat/ph6`: 297 tracked files, ph6 unmerged with phases 6 to 8, template `applicationIdentifier`, MCP unpinned, save schema coupled to the ship. Raised Q1 to Q13. Four phases drafted; `02_guide_rebuild` drafted as a sibling.
- 2026-11-14 : `03_asset_method` drafted as a sibling, from a read of house-renderer's `match-reference` skill and `docs/building_assets.md`. Decided with the user: Blender builders exporting FBX, the industry standard; taken assets under any licence that allows a commercial app with ads. Not a dependency of this folder.
- 2026-11-17 : Unity 6000.4.4f1 with Android installed and licensed on the box (box note `~/repos/plans/2026-11-17_00_unity_editor_headless.md`). On a scratch clone at `91c71db`: clean compile but one warning, EditMode 114/114, PlayMode 3/3 under `xvfb-run -force-vulkan`, `dev` APK built (70 MB, arm64, template package id). Gates moved from human to batch mode on the box, except the device run; Q9 and Q13 recommendations updated. Fixed on this branch: `Tools/build-android.sh` editor auto-detect (NUL separators via `printf`, `-type f` so a directory named `Unity` cannot match), CS4014 at `GameBootstrap.cs:139` (explicit discard). Verified: auto-detect finds `~/Unity/Hub/Editor/6000.4.4f1/Editor/Unity` with `UNITY_PATH` unset; recompile with 0 warnings, PlayMode 3/3.
- 2026-11-17 : Q1 to Q13 answered by the user, mostly the recommendations; Q5 unknown, so b, with keystore and `dev` APK checked on the box. Phase 04, docs and AI workflow review, added at the user's request with an assessment (the docs assume a person verifies every change; now the agent can run the gates itself); the copy becomes phase 05. Q14 (MCP's role) and Q15 (which agent) raised. fala's package id left as an open point in fala-language-tutor.
- 2026-11-17 : Q14 a (batch mode first, MCP the interactive option) and Q15 a (Claude Code first; the Copilot file kept only for consistency, no Copilot workarounds) answered; `04_docs_review.md` updated to match.
- 2026-11-17 : execution started on the user's go. Phase 01 done: `CLAUDE.md`, a Plans section in the instructions and `AGENTS.md`, `Tools/plans.py` vendored, phase 8 items settled per Q5.
- 2026-11-17 : phase 02 done. Audit table with a coverage check seen failing then passing, and a GUID graph. Findings for phase 03: the debug overlay lives only in `SampleScene`, `SampleSceneProfile` is also the URP assets' volume profile, the template's project-wide input actions are unused; folded into `03_pre_split_cleanup.md`.
- 2026-11-17 : phase 03 done in four commits (`453cba9`, `2b9d161`, `78df44a`, `ed02a2f`): MCP pinned, PlayMode test isolation fixed, debug overlay kept, leftovers removed, guide ids set, save system generic. `dev` APK verified with the new id.
- 2026-11-17 : phase 04 implemented in `2425e1f`: `docs/headless-unity.md`, playbook and instructions rewritten for Claude running its own gates, one MCP doc. Open: Q16 (Unity 6.4 is not LTS; the spec says LTS) and the fresh-session check of the done-when.
- 2026-11-17 : Q16 answered: keep 6.4, move to the next LTS when it ships. Spec's engine row corrected.
- 2026-11-17 : phase 04 done: the user accepted the verbatim run of the gate commands instead of a fresh-session check. Branch merged into `main` (fast-forward) for phase 05; `Pitrified/artificial-pirate` created empty by the user.
