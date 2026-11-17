# Repo split - implementation tracking

artificial-pi, the naval game, leaves for its own repo, `artificial-pirate`, written by a Claude session from a plan and the audit.
This repo then becomes the Unity guide, in [`02_guide_rebuild`](../02_guide_rebuild/00_start.md).
Brainstorm, current state, decisions and open questions are in [`00_start.md`](00_start.md).

## Key decisions

Proposed, pending the answers in `00_start.md`:

- **Audit first**, with a script that checks every tracked path is covered.
- **Clean before duplicating**, as far as it can be done without compiling here.
- **The Unity project is copied, not rewritten**: `.meta` GUIDs carry every reference between assets.
- **The guide rebuild is its own folder**, `02_guide_rebuild`; this folder ends when the game runs from its own repo.
- **Building assets from a reference photograph is its own folder**, [`03_asset_method`](../03_asset_method/00_start.md). artificial-pirate gets the method when that folder lands; its pirate-specific findings stay in that repo.
- **Unity checks are human steps.** This box has no Unity Editor; each phase that changes Unity files closes on a check in the Editor or on the device.

## Phases

| #  | Phase                              | Plan                                                 | Status |
| -- | ---------------------------------- | ---------------------------------------------------- | ------ |
| 01 | Land phase 6 work, adopt `plans/`  | [`01_land_ph6.md`](01_land_ph6.md)                   | draft  |
| 02 | Audit                              | [`02_audit.md`](02_audit.md)                         | draft  |
| 03 | Clean up before duplicating        | [`03_pre_split_cleanup.md`](03_pre_split_cleanup.md) | draft  |
| 04 | Write artificial-pirate            | [`04_pirate_repo.md`](04_pirate_repo.md)             | draft  |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-14 : bootstrapped from the flutter split (`flutter-setup-project` `plans/20_repo_split/`, `27_guide_rebuild`, and `fala-language-tutor` `plans/01_bootstrap/`). Read `main` and `origin/feat/ph6`: 297 tracked files, ph6 unmerged with phases 6 to 8, template `applicationIdentifier`, MCP unpinned, save schema coupled to the ship. Raised Q1 to Q13. Four phases drafted; `02_guide_rebuild` drafted as a sibling.
- 2026-11-14 : `03_asset_method` drafted as a sibling, from a read of house-renderer's `match-reference` skill and `docs/building_assets.md`. Decided with the user: Blender builders exporting FBX, the industry standard; taken assets under any licence that allows a commercial app with ads. Not a dependency of this folder.
