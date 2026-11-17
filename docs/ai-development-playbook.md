# AI Development Playbook

## 1. Philosophy

> **The agent writes the change and proves it. The human decides what to build and anything that cannot be undone.**

- The agent works from a plan, implements, runs the gates in [headless-unity.md](headless-unity.md), fixes what fails, and commits only on green.
- A gate the agent has not run is not passed; "looks right" is not a result. A check never seen failing is an assumption, not a gate.
- The human answers the questions the plan raises, decides architecture and scope ([functional-specs.md](functional-specs.md) wins over the agent), plays the build on a device, and does everything irreversible or outward-facing: pushes, signing, Play Console.
- Small atomic commits. No "while I'm here" refactors.

---

## 2. Tooling

| Tool | Purpose |
| ---- | ------- |
| Claude Code | The agent |
| `CLAUDE.md` | Imports `.github/copilot-instructions.md`, the single source of repo instructions (the file keeps its name for consistency with the other repos) |
| `AGENTS.md` | Short entry point for other agents, pointing at the same docs |
| `plans/` | One folder per feature, in the `tracked-development` shape; `python3 Tools/plans.py list` and `check` |
| Unity in batch mode | The agent's way into Unity: compile, tests, builds, captures, editor scripts. See [headless-unity.md](headless-unity.md) |
| Unity MCP | The interactive option when a person is at the Editor on a workstation. See [mcp-setup.md](mcp-setup.md) |

---

## 3. The work loop

1. **Plan.** Non-trivial work gets a folder under `plans/` with its questions, phases and done-when. The human says when planning ends and execution starts.
2. **Review the phase** against the code before touching it: read the files it names, check its claims, add what is missing.
3. **Implement**, inside the phase. Scope comes from the plan, not from a list of files in a chat message.
4. **Run the gates** that apply (§4). On a failure, find the cause and fix it; do not weaken the test.
5. **Commit** on green, one concern per commit, the gate results in the message.
6. **Record** what the phase found in its plan file and the tracking log.
7. **Stop and ask** when a decision is not obvious from the plan, the spec or the code: a product choice, a version, a name the user will see, anything with a cost to undo. State the options and a recommendation, and continue with what does not depend on it.

---

## 4. Gates

| Change | Gates |
| ------ | ----- |
| Any C# or asset change | Compile with 0 errors and 0 warnings, EditMode tests, PlayMode tests |
| Build settings, player settings, packages, Android | Also the `dev` APK |
| Anything visual (materials, shaders, lighting, models, UI layout) | Also a capture, compared against the reference or against a before image |
| New behaviour | A test that fails before the change and passes after |

Commands, and the pitfalls, are in [headless-unity.md](headless-unity.md).
The device run (install, play, `adb logcat -s Unity:V` clean) is the human's.

---

## 5. Changing Unity assets

- Prefer an editor script run in batch mode over a hand edit of scene, prefab or settings YAML: it is repeatable, and Unity writes the file consistently ([headless-unity.md §4](headless-unity.md#4-changing-assets-without-the-editor)).
- Read the YAML diff before committing.
- `ProjectSettings/` changes go through `PlayerSettings` or the relevant API, and only when the plan calls for them.
- When deleting an asset, check its GUID is referenced by nothing.

With MCP at the workstation, the same holds, plus: a clean working tree before any MCP scene edit, one scene or prefab per step, and the PlayMode tests after. See [mcp-setup.md](mcp-setup.md).

---

## 6. Review before commit

The agent reviews its own diff in a fresh pass:

- [ ] Inside the plan phase; nothing unrelated changed
- [ ] No new packages or SDKs without the plan saying so
- [ ] Hard rules in `.github/copilot-instructions.md` hold
- [ ] Gates run and green, results in the commit message
- [ ] New behaviour has a test
- [ ] No secrets, keystores or `build.env` content

---

## 7. When the agent is wrong

- Do not repeat a failed approach. Find the cause, or try a different one.
- If a change contradicts [functional-specs.md](functional-specs.md), the spec wins; changing the spec is the human's decision.
- If a doc and the code disagree, the code is the fact and the doc is the bug: say so and fix the doc.

---

## 8. What the agent must not do

- Add Asset Store packages, third-party SDKs, analytics or ads without the plan saying so.
- Touch `keystore.*`, signing config, `build.env`, Play Console metadata.
- Push, force-push, delete branches, or rewrite history.
- Stage blindly: no `git add -A` over the whole repo; stage the paths the change is about.
