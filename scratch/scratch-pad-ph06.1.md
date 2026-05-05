# Scratch Pad - artificial-pi - Phase 6 - scenes

implementing phase 6 from [tracking.md](docs/tracking.md)

more detailed notes, results, manual steps required, etc go here before being distilled into the final PRs and docs updates

gather additional context before implementing (eg, review relevant docs, load mcp usage guides, etc)

## Relevant docs by task

| Task | Doc reference |
| --- | ----- |
| Ship controller | [systems/ship-controller.md](../docs/systems/ship-controller.md) |
| Camera controller | [systems/camera-controller.md](../docs/systems/camera-controller.md) |
| Coding rules | [coding-standards.md](../docs/coding-standards.md) |
| Project layout / asmdef | [project-structure.md](../docs/project-structure.md) |
| Save data schema | `Scripts/Systems/Save/SaveData.cs` |
| Input interface | `Scripts/Systems/Input/IInputManager.cs`, `MovementInput.cs` |
| Logging | `Scripts/Systems/Logging/Log.cs`, `LogCat.cs` |
| Existing test style | `Tests/EditMode/SaveSystemTests.cs` |

## ship controller

- [x] AI: implement [systems/ship-controller.md](systems/ship-controller.md) (+ tests for input clamping)

### Key design decisions (ship controller)

- `ShipState` - readonly struct in `Game.Gameplay`, returned by `GetState()`.
- `SO_ShipTuning` - ScriptableObject with `[CreateAssetMenu]`; backing fields are `[SerializeField] private`, exposed via read-only properties. Defaults match spec.
- `ShipController` - `sealed` MonoBehaviour; caches `Transform` in `Awake`; reads `IInputManager` injected via `Inject()`; exposes `internal Tick(float dt)` for EditMode tests.
- `AssemblyInfo.cs` added to `Scripts/Gameplay/` mirroring the Systems pattern (`InternalsVisibleTo("Game.Tests.EditMode")`).
- Input validation in `SetInput`: NaN/Infinity values are clamped to 0 and logged as `Log.Warn(LogCat.Gameplay, ...)`. Values outside [-1,1] are clamped silently.

### What was done

Files created (all in `UnitySetupPrpj/Assets/`):

| File | Purpose |
| --- | --- |
| `Scripts/Gameplay/ShipState.cs` | Readonly struct with `Position` (Vector3) + `YawDeg` (float) |
| `Scripts/Gameplay/SO_ShipTuning.cs` | ScriptableObject; `[CreateAssetMenu]`; default values match spec |
| `Scripts/Gameplay/ShipController.cs` | Sealed MonoBehaviour; `Inject`, `SetInput`, `Teleport`, `GetState`, internal `Tick` |
| `Scripts/Gameplay/AssemblyInfo.cs` | `InternalsVisibleTo("Game.Tests.EditMode")` so tests can call `Tick` |
| `Tests/EditMode/ShipControllerTests.cs` | 9 EditMode tests covering NaN, Infinity, range clamping, Teleport, GetState |

Key implementation notes:
- `Awake` caches `transform` into `_transform` (no per-frame property access).
- `Update` reads `IInputManager.GetMovementInput()` then calls `SetInput` then `Tick(Time.deltaTime)`.
- `Tick(float dt)` is `internal` (visible to tests via `InternalsVisibleTo`).
- Forward speed uses `Mathf.MoveTowards` at `Acceleration` m/s^2; steering uses `Mathf.Lerp` at `SteeringResponseLerp` per second.
- Y is locked to `_seaSurfaceY` (default 0) every tick.
- Drift warning logs at `Log.Warn(LogCat.Gameplay, ...)` if ship exceeds 10 km from origin.

### What is missing / manual steps required

1. **Create `SO_ShipTuning` asset instance in Unity Editor:**
   - Assets > Create > artificial-pi > Ship Tuning
   - Save as `Assets/Settings/SO_ShipTuning.asset` (or in a Gameplay subfolder)
   - Values default to spec; adjust as needed.

2. **Create `PF_Ship` prefab in Unity Editor:**
   - New GameObject in Game scene, name it `Ship`.
   - Add `ShipController` component.
   - Drag `SO_ShipTuning` asset into the `_tuning` field.
   - Save as `Assets/Prefabs/PF_Ship.prefab`.
   - (No model/mesh required in v0.2 - a primitive cube works as placeholder.)

3. **Wire InputManager injection in scene:**
   - After the Game scene loads, find the `ShipController` and call `Inject(inputManager)`.
   - This wiring belongs in the scene wiring script (or `GameBootstrap` / a `GameSceneWiring` MonoBehaviour on the Game scene root).

4. **Run EditMode tests:**
   - Window > General > Test Runner > EditMode > run `ShipControllerTests`
   - All 9 tests should pass.

### Manual test checklist (for human)

- [x] Unity recompiles with 0 errors after importing the new scripts.
   - OUTCOME: pass - no compile errors.
- [ ] `SO_ShipTuning` asset created and assigned to `PF_Ship.ShipController._tuning`.
   - OUTCOME: unclear. what is `PF_Ship`? Does it exist at this stage of development? If not, should we create it now or wait until later phases?
- [ ] Press Play in Boot.unity: ship visible in Game scene, moves with keyboard WASD / virtual stick.
   - OUTCOME: unclear.
   - ship is visible but in an all white scene (no water/island yet).
   - virtual stick is not shown.
   - WASD input moves has no visual effect, which could just be due to the lack of water/island context
- [x] Ship does not drift in Y (stays on sea surface plane).
   - OUTCOME: pass - ship Y locked to 0 as expected. as much as we can understand without the water/island context.
- [ ] Pause in Game scene stops ship movement (InputManager disabled).
    - OUTCOME: unclear - pause menu not implemented yet, so can't verify this yet.
- [ ] EditMode Test Runner: all `ShipControllerTests` green.
   - OUTCOME: all fail: UnitySetupPrpj/Logs/TestResults_20260505_213830_shipcontroller.xml

## camera controller

- [ ] AI: implement [systems/camera-controller.md](systems/camera-controller.md)

## stylized water shader

- [ ] AI: create stylized water shader (URP Shader Graph) - minimal animation

## island prefab

- [ ] AI: create island prefab (static mesh, no collisions in v0.2)

## UI overlay

- [ ] AI: implement on-screen virtual stick + pause button (UI Toolkit overlay)

## SaveSystem integration

- [ ] AI: wire SaveSystem into GameManager pause/quit and Menu Continue button

## Manual test: gameplay vertical slice

- [ ] HUMAN: play in Editor - Menu → Game → drive → Pause → Resume → Menu → Continue restores position
