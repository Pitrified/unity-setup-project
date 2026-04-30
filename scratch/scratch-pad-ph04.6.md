# Scratch Pad - artificial-pi

Useful commands accumulated during scaffolding. Will be coalesced into the
appropriate docs later.

---

## Phase 4

### Phase 4.6 - SceneLoader

**Files created:**
- `Assets/Scripts/Core/SceneNames.cs` - scene name constants (Boot, Persistent, Menu, Game)
- `Assets/Scripts/Core/ISceneLoader.cs` - public interface
- `Assets/Scripts/Core/SceneLoader.cs` - MonoBehaviour implementation (Game.Core asmdef)
- `Assets/Scripts/Core/AssemblyInfo.cs` - `InternalsVisibleTo("Game.Tests.EditMode")`
- `Assets/Tests/EditMode/SceneLoaderTests.cs` - 12 EditMode tests

**Key design notes:**
- Always loads additively; never `LoadSceneMode.Single` (keeps Persistent alive).
- Concurrent-call guard: if `_isLoading` is true, returns `_inflight` task unchanged.
- Idempotent: `LoadMenuAsync` is a no-op when Menu is loaded and Game is not.
- Minimum 0.5 s overlay display enforced in `RunTransitionAsync` finally block.
- Input disabled (`IInputManager.SetEnabled(false)`) for entire transition.
- Failure path: logs error, raises `Progress(1f)`, attempts fallback to Menu.
- Cancellation path: safe-unloads any partially loaded scene.
- `ForceSetInflight(Task)` internal helper used only by unit tests.

**Wiring needed (Phase 5):**
- `SceneLoader` component on `Persistent` scene `SystemsRoot` GameObject.
- GameManager calls `sceneLoader.Initialize(inputManager, uiSystem)` in its `Awake`/`Start`.
- Boot scene must register all four scenes (Boot, Persistent, Menu, Game) in Build Settings.
