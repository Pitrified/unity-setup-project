# Copilot Instructions Template

Drop this (trimmed) into `.github/copilot-instructions.md` at the repo root.
Keep it short - it is auto-injected into every Copilot prompt and competes for
context with the user's actual question.

---

```markdown
# Project: artificial-pi (Unity 6 LTS, mobile)

## Source of truth
- Architecture, scope, constraints: `docs/functional-specs.md`
- Folder layout & naming: `docs/project-structure.md`
- Per-system specs: `docs/systems/*.md`
- Coding rules: `docs/coding-standards.md`
- AI rules of engagement: `docs/ai-development-playbook.md`

If a request contradicts these, the docs win.

## Stack (locked)
Unity 6 LTS · URP · IL2CPP · ARM64 · Unity Input System · UI Toolkit ·
kinematic movement · JSON save in `Application.persistentDataPath`.

## Hard rules
- Match existing asmdef boundaries (`Game.Core`, `Game.Systems`, `Game.Gameplay`,
  `Game.UI`, `Game.Editor`).
- No `FindObjectOfType`, `GameObject.Find`, `Resources.Load`, `OnGUI`,
  static mutable state outside designated singletons, magic numbers.
- No new packages, no analytics, no ads, no third-party SDKs.
- No allocations in `Update` (no `new`, no LINQ, no boxing).
- Logging via `Game.Systems.Log`, never `Debug.Log` directly.
- Touch only the files explicitly listed in the request.

## Output style
- Small atomic diffs. One concern per change.
- Public types/members get `///` summary.
- Tests for non-trivial logic in matching `Game.Tests.*` asmdef.
- Ask one clarifying question if scope is ambiguous; do not invent scope.
- No em dashes `–`, `—`, curly quotes, or other "fancy" punctuation.
```

---

## Notes for the human committing this

- Keep total length ≤ 100 lines. Anything longer belongs in `docs/`.
- Do not duplicate the spec - link to it. The agent has access to the repo.
- Re-evaluate after every milestone: rules that are obvious to the AI by then can be removed.