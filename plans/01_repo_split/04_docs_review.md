---
status: draft
---

# Phase 04 - Review the docs and the AI workflow before duplicating

## Overview

The user, 2026-11-17: "add in the phases a general review of the docs and the template. For example, the AI and MCP workflow might be a bit stale with the new AI-assisted heavy self corrections. Assess."
The docs both repos get are reviewed here, once, so artificial-pirate copies the current workflow rather than the April one.
Guide-only docs and the runtime as a template are reviewed in [`02_guide_rebuild`](../02_guide_rebuild/00_start.md).
Context: [`00_start.md`](00_start.md), after [`03_pre_split_cleanup.md`](03_pre_split_cleanup.md) and before [`05_pirate_repo.md`](05_pirate_repo.md). Decided by `00_start.md` Q14 (batch mode first, MCP as the interactive option) and Q15 (Claude Code first), 2026-11-17.

## Assessment, read 2026-11-17

Read: `docs/ai-development-playbook.md`, `docs/unity-mcp-best-practices.md`, `docs/unity-mcp-guide.md`, `docs/mcp-setup.md`, `docs/testing-strategy.md`, `docs/dev-environment-setup.md`, `docs/README.md`, `AGENTS.md`, `.github/copilot-instructions.md`.
The docs were written in April and May 2026, around one assumption: the agent writes code, a person verifies it.
That was true then, since nothing could compile or test without a person at the Editor. Since 2026-11-17 it is not.

### Stale

1. **The person is the loop.** The playbook's philosophy is "AI generates, human reads the diff, human runs it"; MCP mitigation 3 is "open Unity and visually verify every scene change"; the review checklist is a person's.
   Now the agent can close its own loop on the box: compile with 0 warnings, EditMode and PlayMode tests, a capture under `xvfb-run ... -force-vulkan`, the `dev` APK.
   The workflow should say the agent runs those gates and corrects itself until they pass, and that the person's part is the decisions, the device feel, and anything irreversible.
2. **Copilot first.** The tooling stack, `dev-environment-setup.md` and `unity-mcp-guide.md` (subtitled "Ubuntu + VSCode + GitHub Copilot") are written for Copilot Agent mode in VS Code.
   There is no `CLAUDE.md`, so a Claude Code session does not load `.github/copilot-instructions.md`; the user-level convention is a `CLAUDE.md` that imports it.
   `copilot-instructions.md` ends with a Copilot-only "Permanent chat" rule (`#askQuestions` after every task).
3. **Scope by file list.** "Touch only files explicitly listed in the request" (both instruction files) and the three-block prompting pattern assume one small request at a time.
   A self-correcting agent working a plan needs scope from the plan instead: the `plans/` convention (tracked-development) sets the phase, its files and its done-when.
4. **`dotnet build` as the compile check.** The playbook's level table and checklist name it; the real check is a batch-mode compile, which is what the box now runs.
5. **MCP assumes an open Editor.** MCP is the agent's only way into Unity in these docs, and it needs the Editor GUI running on the workstation.
   On the box the way in is batch mode: `-executeMethod` for editor scripts, `-runTests`, `-buildTarget`. Settled by `00_start.md` Q14: batch mode first, MCP for interactive work at the workstation.
6. **The future section is partly done or superseded.** "Unity Test Runner in CI" is now a batch run on the box; "an AI changelog" is what a plan folder's log already is; "`Tools/setup.sh`" exists.
7. **Plans in two places.** `docs/tracking.md` and `scratch/` alongside `plans/`; Q12 settled that history stays here and the open phases move.

### Inconsistent

8. **Hard rule vs. code.** The rules forbid `FindObjectOfType`, while `GameBootstrap` uses `FindAnyObjectByType` to wire the Persistent scene. Either the composition root is a named exception or the wiring changes.
9. **"Unity 6 LTS".** The docs say LTS throughout; the project is on `6000.4.4f1`. Whether 6000.4 is an LTS release is to be checked against Unity's release list, and the docs say the real version either way.

### Still sound

- The architecture rules (asmdef boundaries, no allocations in `Update`, `Game.Systems.Log`), tests with non-trivial logic, small atomic diffs, the "what AI must not do" list (signing, Play Console, force pushes, history rewrites).
- The MCP risk list (scene and prefab corruption, irreversible asset changes) and commit-before-scene-edit.
- `unity-mcp-best-practices.md`'s debugging loop. Its AudioMixer section is this game's session notes, so it goes with artificial-pirate, not the guide.

## Plan

- Rewrite `docs/ai-development-playbook.md` around the agent's own loop: plan folder for scope, the batch gates the agent runs (with the exact commands), self-correction until they pass, the human steps that remain.
- `CLAUDE.md` importing `.github/copilot-instructions.md` (`00_start.md` Q15). The Copilot file stays only for consistency with the other repos: written for Claude, no Copilot-only rules or workarounds, so the "Permanent chat" rule goes. Copilot-first setup steps in `dev-environment-setup.md` and `unity-mcp-guide.md` go too.
- Replace "touch only files listed in the request" with "stay inside the current plan phase"; keep "no while-I'm-here refactors".
- `docs/testing-strategy.md`: the headless commands as run on the box, including `xvfb-run ... -force-vulkan` for PlayMode.
- Batch mode as the default way into Unity (`00_start.md` Q14): editor scripts by `-executeMethod`, tests, builds and captures, with the commands. MCP documented as the interactive option at the workstation Editor; `unity-mcp-guide.md` trimmed to the server actually used, set up for Claude Code.
- Fix 8 and 9.
- `docs/README.md` and `AGENTS.md` updated to the result.

## Out of scope

- Guide-only docs (setup from zero, the pattern index) and the runtime as a template: `02_guide_rebuild`.
- Game docs (`functional-specs.md`, `docs/systems/`): they move with the game in phase 05.

## Done when

- Every stale item above is changed or has a line saying why it stays.
- The link check passes.
- A fresh Claude Code session on the box, given only the repo, finds the gates and runs them (EditMode, PlayMode, `dev` APK) from the docs alone.

## What the implementation found
