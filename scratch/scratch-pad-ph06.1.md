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
- `Awake` caches `transform` into `_transform`. `Tick`, `Teleport`, and `GetState` also contain a lazy-init guard (`if (_transform == null) _transform = transform`) because Unity's EditMode test runner does not call `Awake` when `AddComponent` is used outside Play Mode.
- `Update` reads `IInputManager.GetMovementInput()` then calls `SetInput` then `Tick(Time.deltaTime)`.
- `Tick(float dt)` is `internal` (visible to tests via `InternalsVisibleTo`).
- Forward speed uses `Mathf.MoveTowards` at `Acceleration` m/s^2; steering uses `Mathf.Lerp` at `SteeringResponseLerp` per second.
- Y is locked to `_seaSurfaceY` (default 0) every tick.
- Drift warning logs at `Log.Warn(LogCat.Gameplay, ...)` if ship exceeds 10 km from origin.

### What is missing / manual steps required

1. ~~Create `SO_ShipTuning` asset~~ - DONE via MCP: `Assets/Settings/SO_ShipTuning.asset`
2. ~~Create `PF_Ship` prefab~~ - DONE via MCP: `Assets/Prefabs/PF_Ship.prefab` (from existing Ship GO in Game scene). Tuning is baked into prefab.
3. ~~Wire InputManager injection in scene~~ - DONE via MCP: `GameSceneWiring.cs` in `Scripts/Core/`, placed in Game scene, `_ship` and `_camera` fields wired to correct components. Runs `Inject` and `SetTarget/SnapToTarget` in `Start`.
4. **Verify in Inspector:** Open Game.unity, select `GameSceneWiring` object - confirm `_ship` = Ship.ShipController, `_camera` = MainCamera.CameraController.
5. **Run EditMode tests:**
   - Window > General > Test Runner > EditMode > run `ShipControllerTests` (9 tests)
   - Already passing - confirm unchanged.

### Manual test checklist (for human)

- [x] Unity recompiles with 0 errors after importing the new scripts.
   - OUTCOME: pass - no compile errors.
- [ ] `SO_ShipTuning` asset created and assigned to `PF_Ship.ShipController._tuning`.
   - OUTCOME: seems ok
- [ ] Press Play in Boot.unity: ship visible in Game scene, moves with keyboard WASD / virtual stick.
   - OUTCOME: promising.
   - ship is visible but in an all white scene (no water/island yet).
   - virtual stick is not shown.
   - WASD input moves moves camera and ship ok
- [x] Ship does not drift in Y (stays on sea surface plane).
   - OUTCOME: pass - ship Y locked to 0 as expected. as much as we can understand without the water/island context.
- [ ] Pause in Game scene stops ship movement (InputManager disabled).
    - OUTCOME: unclear - pause menu not implemented yet, so can't verify this yet.
- [x] EditMode Test Runner: all `ShipControllerTests` green.
   - OUTCOME: all pass

## camera controller

- [x] AI: implement [systems/camera-controller.md](systems/camera-controller.md)

### Key design decisions (camera controller)

- `SO_CameraTuning` - ScriptableObject; `LocalOffset=(0,3,-6)`, `PositionLerp=5`, `RotationLerp=5`, `LookAtTarget=true`.
- `CameraController` - `sealed` MonoBehaviour on `MainCamera`. `SetTarget(Transform)`, `SnapToTarget()` public. `internal Tick(float dt)` for tests. Runs in `LateUpdate`.
- Smoothing: `1 - Mathf.Exp(-k * dt)` for framerate-independent position Lerp and rotation Slerp.
- Null target: holds last position, logs `Log.Warn` once per null episode (reset when target becomes non-null or `SetTarget` is called again).

### What was done

Files created (all in `UnitySetupPrpj/Assets/`):

| File | Purpose |
| --- | --- |
| `Scripts/Gameplay/SO_CameraTuning.cs` | ScriptableObject; `[CreateAssetMenu]`; default values match spec |
| `Scripts/Gameplay/CameraController.cs` | Sealed MonoBehaviour; `SetTarget`, `SnapToTarget`, internal `Tick` |
| `Tests/EditMode/CameraControllerTests.cs` | 7 EditMode tests covering SnapToTarget, null target, position smoothing, SetTarget |

Key implementation notes:
- Same lazy-init guard pattern as ShipController: `if (_transform == null) _transform = transform` at the top of `Tick`, `SnapToTarget`.
- Position smoothing: `t = 1 - Mathf.Exp(-PositionLerp * dt)`, then `Vector3.Lerp`.
- Rotation smoothing: `t = 1 - Mathf.Exp(-RotationLerp * dt)`, then `Quaternion.Slerp` toward `Quaternion.LookRotation(target - camera)`.
- `SnapToTarget` immediately places camera at `target.TransformPoint(LocalOffset)` and calls `LookAt` if `LookAtTarget` is true.

### What is missing / manual steps required

1. ~~Create `SO_CameraTuning` asset~~ - DONE via MCP: `Assets/Settings/SO_CameraTuning.asset`
2. ~~Wire `CameraController` in Game scene~~ - DONE via MCP: component added to MainCamera, tuning assigned
3. ~~Create `GameSceneWiring` object~~ - DONE via MCP: GameObject in Game scene with _ship + _camera wired
4. **Verify in Inspector:** Open Game.unity, select `GameSceneWiring` object - confirm `_ship` = Ship.ShipController, `_camera` = MainCamera.CameraController.
5. **Run EditMode tests:**
   - Window > General > Test Runner > EditMode > run `CameraControllerTests`
   - All 7 tests should pass.

### Manual test checklist (for human)

- [x] Unity recompiles with 0 errors after importing the new scripts.
- [x] `SO_CameraTuning` asset created and assigned to `MainCamera.CameraController._tuning`.
- [x] Press Play in Boot.unity: camera follows ship smoothly from behind-and-above.
- [x] Camera snaps to correct offset after scene load (no jarring slide from origin).
- [x] Camera rotates to look at ship while ship turns.
- [x] EditMode Test Runner: all `CameraControllerTests` green.

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
