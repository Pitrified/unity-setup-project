# Scratch Pad - artificial-pi - Phase 5 - scenes

implementing phase 5 from [tracking.md](docs/tracking.md)

more detailed notes, results, manual steps required, etc go here before being distilled into the final PRs and docs updates

gather additional context before implementing (eg, review relevant docs, load mcp usage guides, etc)

## Relevant docs by task

| Task | Doc reference |
|------|---------------|
| wire `Boot.unity` | [systems/game-bootstrap.md](../docs/systems/game-bootstrap.md) - flow, constraints, single GO rule |
| wire `Persistent.unity` | [systems/game-manager.md](../docs/systems/game-manager.md), [systems/scene-loader.md](../docs/systems/scene-loader.md), [systems/audio-manager.md](../docs/systems/audio-manager.md), [systems/input-manager.md](../docs/systems/input-manager.md), [systems/save-system.md](../docs/systems/save-system.md), [systems/ui-system.md](../docs/systems/ui-system.md) |
| wire `Menu.unity` | [systems/game-manager.md](../docs/systems/game-manager.md) §Public interface - Play/Continue/Quit entry points, [systems/ui-system.md](../docs/systems/ui-system.md) §Screens |
| wire `Game.unity` | [systems/ship-controller.md](../docs/systems/ship-controller.md), [systems/camera-controller.md](../docs/systems/camera-controller.md), [project-structure.md](../docs/project-structure.md) §2 (sea plane, PF_Ship prefab) |
| register scenes in Build Settings | [project-structure.md](../docs/project-structure.md) §2 - scene list: Boot, Persistent, Menu, Game (Boot index 0) |

## Scene Creation

- [x] HUMAN: create `Boot.unity`, `Persistent.unity`, `Menu.unity`, `Game.unity` (commit empty) -- done via MCP 2026-05-03

## wire boot scene

- [x] AI: wire `Boot.unity` (single `GameBootstrap`) -- done via MCP 2026-05-04

### What was implemented (MCP)
- Loaded `Assets/Scenes/Boot.unity` (was empty)
- Created one GameObject named `GameBootstrap` at origin
- Added `Game.Core.GameBootstrap` component (no serialized fields - component is self-contained)
- Saved the scene
- Console verified clean (0 errors, 0 warnings)

### Final Boot.unity hierarchy
```
Boot.unity
└── GameBootstrap  [Transform, GameBootstrap]
```

### Manual steps still to do
- None for this scene. The `GameBootstrap` script handles all boot logic at runtime.
- The Persistent scene must be wired (next task) before Boot→Persistent handoff works.
- Boot.unity must be added to Build Settings at index 0 (handled in the "register scenes" task).

## wire persistent scene

- [ ] AI: wire `Persistent.unity` (GameManager, all systems, UIRoot)

## wire menu scene

- [ ] AI: wire `Menu.unity` (Play / Continue / Quit buttons routed through GameManager)

## wire game scene

- [ ] AI: wire `Game.unity` minimum (sea plane, ship spawn, camera)

## register scenes

- [ ] AI: register all scenes in Build Settings (Boot first)

## test in editor

- [ ] HUMAN: press Play in `Boot.unity` - expect Menu reachable, no console errors
