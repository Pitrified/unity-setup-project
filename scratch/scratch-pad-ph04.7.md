# Scratch Pad - artificial-pi

Useful commands accumulated during scaffolding. Will be coalesced into the
appropriate docs later.

---

## Phase 4

### Phase 4.7 - GameManager

- [x] AI: implement [systems/game-manager.md](systems/game-manager.md) (+ state-machine EditMode tests)

#### Files created

| File | Assembly | Notes |
|------|----------|-------|
| `Assets/Scripts/Core/GameState.cs` | `Game.Core` | Enum: Boot, Loading, Menu, Playing, Paused |
| `Assets/Scripts/Core/IGameManager.cs` | `Game.Core` | Contract; used by bootstrap and UI controllers |
| `Assets/Scripts/Core/GameManager.cs` | `Game.Core` | Full implementation; MonoBehaviour singleton |
| `Assets/Tests/EditMode/GameManagerTests.cs` | `Game.Tests.EditMode` | 26 EditMode tests for state machine |

#### Manual steps required

1. **Open Unity Editor** - let it recompile; confirm 0 errors / 0 warnings in the Console.
2. **Run EditMode tests** - Window > General > Test Runner > EditMode tab > Run All.
   All 26 `GameManagerTests` should be green.

No new scenes, packages, or assets are needed at this phase. `GameManager.Initialize()` is
called by `GameBootstrap` (next item in Phase 4); the GameObject wiring happens in Phase 5
when `Persistent.unity` is set up.
