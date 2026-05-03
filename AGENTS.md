# AGENTS.md

This is the `artificial-pi` Unity 6 LTS mobile project (v0.2, private alpha).

## Read these docs before generating any code

| Priority | File | Purpose |
|----------|------|---------|
| 1 | [docs/ai-development-playbook.md](docs/ai-development-playbook.md) | Rules of engagement, prompting pattern |
| 2 | [docs/functional-specs.md](docs/functional-specs.md) | Locked architecture & scope |
| 3 | [docs/project-structure.md](docs/project-structure.md) | Folder layout & asmdef boundaries |
| 4 | [docs/coding-standards.md](docs/coding-standards.md) | C# style rules |
| 5 | [docs/systems/](docs/systems/) | One spec file per runtime system |
| 6 | [docs/tracking.md](docs/tracking.md) | Current phase & checklist |
| 7 | [docs/unity-mcp-best-practices.md](docs/unity-mcp-best-practices.md) | Unity MCP session tips, recovery patterns, validation workflow |

## Non-negotiable constraints

- Unity 6 LTS, URP, IL2CPP, ARM64 only.
- asmdef boundaries: `Game.Core`, `Game.Systems`, `Game.Gameplay`, `Game.UI`, `Game.Editor`.
- No `FindObjectOfType`, `GameObject.Find`, `Resources.Load`, `OnGUI`, magic numbers.
- No new packages, analytics, ads, or third-party SDKs.
- No allocations in `Update`. No LINQ in hot paths.
- Logging via `Game.Systems.Log` only; never `Debug.Log` directly.
- Touch only files explicitly listed in the request.
- Tests for non-trivial logic in the matching `Game.Tests.*` asmdef.

Full rules: [docs/ai-development-playbook.md](docs/ai-development-playbook.md).
