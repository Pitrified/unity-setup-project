# Project: artificial-pi (Unity 6, mobile)

Canonical instructions for agents working in this repo, imported by `CLAUDE.md`.
The file keeps its Copilot name for consistency with the other repos; it is written for Claude Code.

## Source of truth
- Architecture, scope, constraints: `docs/functional-specs.md`
- Folder layout & naming: `docs/project-structure.md`
- Per-system specs: `docs/systems/*.md`
- Coding rules: `docs/coding-standards.md`
- How the agent works (loop, gates, what it must not do): `docs/ai-development-playbook.md`
- Running Unity headless (install, commands, pitfalls): `docs/headless-unity.md`
- Unity MCP, the interactive option at the Editor: `docs/mcp-setup.md`, `docs/unity-mcp-best-practices.md`

If a request contradicts these, stop and ask. If a doc contradicts the code, the code is the fact and the doc is the bug: say so and fix the doc.

## Plans

New work is planned in `plans/NN_name/`, in the shape the `tracked-development` skill describes:
`00_start.md` for the brainstorm and questions, `tracking.md` for the phases and log, one file per phase.
`python3 Tools/plans.py list` shows the folders and their status; `python3 Tools/plans.py check` checks them.
`Tools/plans.py` is a vendored copy of the skill's script; to update it, copy the skill's file over it.
`docs/tracking.md` and `scratch/` are the diary from before `plans/`, kept as history.

## Gates

Every change closes on the gates in `docs/headless-unity.md` §3, run by the agent in batch mode:
compile with 0 errors and 0 warnings, EditMode and PlayMode tests green, the `dev` APK for build-affecting changes, a capture for visual ones.
Results go in the commit message. The device run is the human's.

## Stack
Unity `6000.4.4f1` (exact version in `UnitySetupPrpj/ProjectSettings/ProjectVersion.txt`) · URP · IL2CPP · ARM64 ·
Unity Input System · UI Toolkit · kinematic movement · JSON save in `Application.persistentDataPath`.

## Hard rules
- Match existing asmdef boundaries (`Game.Core`, `Game.Systems`, `Game.Gameplay`,
  `Game.UI`, `Game.Editor`).
- No `FindObjectOfType`/`FindAnyObjectByType`, `GameObject.Find`, `Resources.Load`, `OnGUI`,
  static mutable state outside designated singletons, magic numbers.
  The one exception is `GameBootstrap`, the composition root: it finds the Persistent scene's systems once at boot, since references cannot cross scenes.
- No new packages, no analytics, no ads, no third-party SDKs, unless the plan says so.
- No allocations in `Update` (no `new`, no LINQ, no boxing).
- Logging via `Game.Systems.Log`, never `Debug.Log` directly.
- Stay inside the current plan phase. No "while I'm here" refactors.
- Scenes, prefabs and settings change through Unity (an editor script in batch mode, or MCP at the Editor), not by hand-editing YAML.

## Output style
- Small atomic commits. One concern per commit.
- Public types/members get `///` summary.
- Tests for non-trivial logic in matching `Game.Tests.*` asmdef; a failing test before its fix where possible.
- Stop and ask on a decision that is not obvious from the plan, the spec or the code.
- No em dashes `--`, `---`, curly quotes, or other fancy punctuation.
