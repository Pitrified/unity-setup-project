# AI Development Playbook

## 1. Philosophy

> **AI writes ~80-90% of the code. The human enforces architecture, scope, and correctness.**

Non-negotiables:

- AI generates → human reads the diff → human runs it.
- Architecture decisions live in [functional-specs.md](functional-specs.md). AI does not change them.
- Small atomic changes. No "while I'm here" refactors.

---

## 2. Tooling stack

| Tool                                | Purpose                                  |
| ----------------------------------- | ---------------------------------------- |
| VS Code                             | Editor                                   |
| GitHub Copilot (Chat + Agent mode)  | Primary AI assistant                     |
| Unity MCP Server                    | Lets the agent read/write Unity project state - see [mcp-setup.md](mcp-setup.md) |
| `.github/copilot-instructions.md`   | Repo-wide AI instructions (auto-loaded)  |
| `AGENTS.md` (repo root)             | Entry point for OpenAI/Claude/etc agents |

---

## 3. Repository AI configuration files

Create and keep current:

```
<repo>/
├── .github/
│   └── copilot-instructions.md     # auto-injected into Copilot prompts
├── .vscode/
│   └── mcp.json                    # MCP server registrations
├── AGENTS.md                       # agent-agnostic onboarding
└── docs/                           # what humans + AI read
```

`copilot-instructions.md` and `AGENTS.md` should be **short** (≤ 100 lines each)
and link into `docs/` rather than duplicating content. A starter is in
[copilot-instructions-template.md](copilot-instructions-template.md).

---

## 4. MCP integration

The Unity MCP server gives the agent Editor capabilities:

- Read scene/prefab hierarchy
- Create GameObjects, attach components, set serialized fields
- Trigger Editor methods (build, play, test)

**Risks:** scene/prefab corruption, hidden side effects, irreversible asset
changes.

**Mitigations (mandatory):**

1. **Commit before any MCP-driven scene edit.** Treat scenes as binary; rely on git to revert.
2. **One system at a time.** Never let the agent touch multiple scenes/prefabs in a single turn.
3. **Open Unity and visually verify** every scene change before committing.
4. **Run PlayMode tests** after any scene/prefab change.
5. **Forbid mass operations** (delete, rename, restructure) without explicit human-issued prompt.

Setup details: [mcp-setup.md](mcp-setup.md).

---

## 5. AI usage levels

| Level | Scope                       | Review cost | Notes                          |
| ----- | --------------------------- | ----------- | ------------------------------ |
| 1     | Pure C# scripts             | Low         | Default mode                   |
| 2     | System wiring (asmdefs, DI) | Medium      | Verify with `dotnet build`     |
| 3     | Scene/prefab edits via MCP  | High        | Commit-first, test after       |
| 4     | Build/release scripts       | High        | Run on a throwaway build first |

---

## 6. Prompting pattern

Always include three blocks:

```
## Context
Unity 6 LTS, URP, mobile, kinematic movement, no physics.
Architecture: see docs/functional-specs.md and the relevant systems/*.md.

## Task
<one concrete change>

## Constraints
- Touch only files: <list>
- No new dependencies
- Match existing naming + asmdef boundaries
```

Anti-patterns:

- "Improve the code" / "Make it better" → too vague, will hallucinate scope.
- "Implement the whole save system" → too large, split per public method.
- Pasting a stack trace with no context → ask AI to ask clarifying questions first.

---

## 7. Review checklist (per AI diff)

Before accepting:

- [ ] Touches only the files in scope
- [ ] No new packages / using directives without reason
- [ ] No `Find*`, `Resources.Load`, `OnGUI`, magic numbers
- [ ] Compiles (`dotnet build` on the asmdef, or full Unity recompile)
- [ ] Existing tests pass; new behavior has at least one test
- [ ] No secrets, keystores, or `.env` content committed

---

## 8. When AI is wrong

- Do **not** keep re-prompting on the same failed approach. Ask for an alternative.
- If the agent contradicts `functional-specs.md`, the spec wins. Update the spec **only** as a deliberate human decision.
- If a generated change is hard to understand in <2 minutes, reject it and ask for a smaller version.

---

## 9. What AI must not do (this project)

- Edit `ProjectSettings/*.asset` without explicit instruction.
- Add Asset Store packages, third-party SDKs, analytics, or ads.
- Touch `keystore.*`, signing config, Play Console metadata.
- Run `git push --force`, delete branches, or rewrite history.
- Run `git add -A` blindly; always stage explicitly.

---

## 10. Long-term evolution (post v0.2)

- Promote local build script to GitHub Actions CI.
- Add Unity Test Runner to CI (EditMode + PlayMode on Linux runner).
- Add an AI changelog: every accepted AI diff appended to `docs/changelog.md`.
