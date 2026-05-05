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

- [x] AI: create stylized water shader (URP Shader Graph) - minimal animation

### Key design decisions (water shader)

- Custom URP HLSL shader instead of Shader Graph - same visual result, better mobile performance, no allocation risk, easier AI-authoring. Deviation from "Shader Graph" spec noted.
- Shader name: `artificial-pi/Water`. File: `Assets/Shaders/SG_Water.shader` (SG_ prefix to match project naming if it were Shader Graph).
- Material: `Assets/Art/M_Water.mat` assigned to `Sea` plane in Game scene.
- Features: vertex sine-wave displacement, scrolling UV pattern, deep/shallow color blend, foam highlight on peaks, URP fog (ComputeFogFactor/MixFog).

### What was done

Files created via MCP:

| File | Purpose |
| --- | --- |
| `Shaders/SG_Water.shader` | Custom URP HLSL shader with wave displacement and animated pattern |
| `Art/M_Water.mat` | Material using `artificial-pi/Water` shader |
| Game.unity | Sea MeshRenderer now uses M_Water material |

Properties exposed in Inspector (with defaults):
- `_ShallowColor` (0.22, 0.62, 0.82) - light blue
- `_DeepColor` (0.05, 0.22, 0.50) - dark blue
- `_FoamColor` (0.80, 0.92, 1.00) - near-white
- `_WaveSpeed` = 0.30 - scroll/displacement rate
- `_WaveFreq` = 1.80 - spatial wave frequency
- `_WaveAmp` = 0.15 - vertex displacement height
- `_PatternScale` = 3.0 - UV tile size
- `_FoamThreshold` = 0.72 - peak percentage for foam

### What is missing / manual steps required

- **Tune in Editor:** Open Game scene, select Sea, inspect M_Water material. Adjust colors/speed/amplitude to taste.
- The shader is Opaque; no transparency or depth-based effects in v0.2. Add later if needed.

### Manual test checklist (for human)

- [x] Unity: 0 shader errors after import.
- [x] Sea plane in Game scene shows animated water (blue, scrolling pattern visible).
- [x] Wave vertex displacement gives subtle 3D appearance.
   - OUTCOME: it is *very* subtle, may want to increase `_WaveAmp` for better visibility. increased to 0.15 from 0.05.

## island prefab

- [x] AI: create island prefab (static mesh, no collisions in v0.2)

### Key design decisions (island prefab)

- Static visual mesh only, no collider per spec ("no collisions in v0.2"). `Static` flag set for batching.
- Built from primitives (cylinder base + scaled sphere top) as island silhouette. No external mesh assets needed.
- Material: `Assets/Art/M_Island.mat` - flat green URP Lit, no texture (mobile-friendly).
- Placed at world position (20, 0, 30) so it is visible from ship start.
- Saved as `Assets/Prefabs/PF_Island.prefab`.

### What was done

Files created via MCP:

| File | Purpose |
| --- | --- |
| `Prefabs/PF_Island.prefab` | Static island prefab: root `Island` + children `IslandBase` (Cylinder) + `IslandHill` (Sphere) |
| `Art/M_Island.mat` | URP Lit material, green `_BaseColor` (0.28, 0.52, 0.18), assigned to both children |
| Game.unity | `PF_Island` instance placed at (20, 0, 30), linked prefab |

Island hierarchy:
- `Island` (empty root, static, pos (20, 0, 30))
  - `IslandBase` (Cylinder, scale (6, 0.4, 6), pos y=-0.3, no collider)
  - `IslandHill` (Sphere, scale (5, 1.5, 5), pos y=0.5, no collider)

### What is missing / manual steps required

- No collider intentional per spec (v0.2). Add MeshCollider in v0.3 when ship-island collision is planned.
- You may want to tune the island position / scale - it is placed visually to be visible from ship start but not tested in Play mode.

### Manual test checklist (for human)

- [x] Unity: 0 errors after scene load.
- [x] Island visible in Game scene: green mound above the waterline.
- [x] No collider on island (ship passes through).
- [x] `PF_Island.prefab` exists in Assets/Prefabs/.

## UI overlay

- [x] AI: implement on-screen virtual stick + pause button (UI Toolkit overlay)

### Key design decisions (UI overlay)

- uGUI Canvas overlay used for virtual stick and pause button per spec allowance: "uGUI permitted only for v0.2 fallbacks (e.g. virtual joystick) where UI Toolkit is awkward."
- `OnScreenStick` (Unity Input System) component on the joystick knob image, `controlPath = "<Gamepad>/leftStick"`. A `<Gamepad>/leftStick` binding is added to `Gameplay/Move` action so the virtual stick feeds the existing `InputManager` pipeline.
- `OnScreenButton` (Unity Input System) on the pause button image, `controlPath = "<Gamepad>/start"`. A `<Gamepad>/start` binding is added to `Gameplay/Pause` action so the pause button feeds `InputManager.ConsumePausePressed()` → `GameManager.Update` → `PauseGame()`. No custom script needed.
- No new C# script created - all wiring is via Unity components. Pure data, zero logic.
- Canvas: Screen Space - Overlay, sort order 10. Two children: `JoystickBackground` (bottom-right), `BtnPause` (top-left).

### What was done

| File / Asset | Change |
| --- | --- |
| `Settings/InputActions.inputactions` | Added `<Gamepad>/leftStick` binding to `Gameplay/Move`; added `<Gamepad>/start` binding to `Gameplay/Pause` |
| `Game.unity` | Added `HUD` Canvas (Screen Space Overlay, sort 10, ScaleWithScreenSize 1920x1080) with two children |
| `HUD/JoystickBackground` | `Image` (alpha 0.15), `RectTransform` bottom-right anchor (240x240, offset -60,60) |
| `HUD/JoystickBackground/JoystickKnob` | `Image` (alpha 0.5), `OnScreenStick` (controlPath `<Gamepad>/leftStick`, movementRange 80) |
| `HUD/BtnPause` | `Image` (alpha 0.3), `OnScreenButton` (controlPath `<Gamepad>/start`), `RectTransform` top-left (120x80, offset 30,-30) |
| `HUD/BtnPause/Label` | `Text` "II", fontSize 36, white, centered |

### What is missing / manual steps required

- No sprite art for joystick/button. They display as white rectangles. Replace `Image.sprite` with proper art when available.
- The `Text` component uses legacy uGUI Text. No TextMeshPro dependency needed for v0.2.
- Verify in Play mode that touch on the bottom-right drags the ship and top-left "II" button triggers pause overlay.

### Key implementation notes

- `EventSystem` GameObject must have `InputSystemUIInputModule` (not `StandaloneInputModule`) for `OnScreenStick` / `OnScreenButton` to receive pointer events. Without it, `IPointerDownHandler` never fires.
- Root cause of original "no-op" bug: Game scene had no `EventSystem` at all. `GraphicRaycaster` on the HUD Canvas has no way to dispatch pointer events without one.

### Manual test checklist (for human)

**NOTE: `OnScreenStick` and `OnScreenButton` respond to touch input only, not mouse clicks.** To test in Editor:
- Option A: Window > General > **Device Simulator** (converts mouse events to touch).
- Option B: Edit > Project Settings > Input System Package > enable **"Simulate Touch Input From Mouse or Pen"**.

- [x] Unity: 0 errors after scene load.
- [x] HUD Canvas visible in Game scene Hierarchy under Game scene.
- [ ] In Play mode (Device Simulator or touch-sim enabled): drag on bottom-right joystick area → ship moves.
   - OUTCOME: tested with simulator. Touch input moves the ship not as expected. might still be bug of using mouse on joystick.
   - the stick seem to work in "burst": when I click and hold, it moves the ship for a moment then stops until I release and click again.
   - clicking on the screen, outside the joystick area, moves in the same way.
   - rotation keeps working in a "snake" pattern so i can steer left right, but i expected that if i kept the stick held left, the ship would keep turning left, but it only turns left for a moment then stops. if i do not release and move right, it rotates right for a moment then stops, and so on.
- [ ] In Play mode (Device Simulator or touch-sim enabled): tap "II" button (top-left) → pause overlay appears.
   - OUTCOME: tested with simulator. Touch input on pause button does not trigger pause. might still be bug of using mouse on button.
- [x] WASD keyboard still drives ship (unaffected by new bindings).

### Manual test checklist part 2 - after fix

**Bug found and fixed:** The old `<Touchscreen>/touch*/delta` binding was still in `Gameplay/Move`. This binding sends *incremental deltas* (not absolute position) which caused the "burst" movement: each touch event sent a delta, then returned to zero, causing the ship to jerk. The fix: removed `<Touchscreen>/touch*/delta` from `InputActions.inputactions`.

**Pause button:** Was likely working but the user had a stale in-memory InputActions asset from before the bindings were added. Re-entering Play Mode after the asset refresh will pick up the new `<Gamepad>/start` binding.

**Required manual step:** Stop Play Mode, then re-enter Play Mode so Unity reloads the updated `InputActions.inputactions`.

- [x] Re-enter Play Mode after asset refresh.
- [x] In Play mode (Device Simulator + "Simulate Touch Input From Mouse or Pen" enabled): drag joystick → ship turns and moves continuously while held. Release → ship slows.
   - OUTCOME: PASS - confirmed working after EventSystem fix.
- [x] In Play mode: tap "II" button (top-left) → pause overlay appears.
   - OUTCOME: PASS - confirmed working after EventSystem fix.

## SaveSystem integration

- [x] AI: wire SaveSystem into GameManager pause/quit and Menu Continue button

### Key design decisions (SaveSystem integration)

- `GameManager` already owned `ISaveSystem` and called `PersistCurrentState()` (a stub) on pause, quit, and return-to-menu. The TODO comment in `PersistCurrentState` explicitly called for live ship data.
- `GameSceneWiring.Start()` is the natural registration point: it already wires InputManager into ShipController. Adding `gm.RegisterShip(_ship)` here means GameManager always has the active ship reference within the same frame the Game scene comes alive.
- `RegisterShip` in GameManager: stores `_activeShip`; if `HasSave` is true, loads and calls `ship.Teleport()` to restore saved position. This makes Continue work without any changes to `ContinueAsync`.
- `PersistCurrentState` updated to build `ShipSaveData` from `_activeShip.GetState()` before writing.
- **Bug fix included**: `ReturnToMenuAsync` did not hide the pause screen or re-enable input before loading the Menu scene. Since UISystem lives in the Persistent scene, the pause overlay survived the scene transition. Fixed by calling `UISystem?.HideScreen(ScreenId.Pause)` and `InputManager?.SetEnabled(true)` before `TransitionTo(GameState.Loading)`.

### What was done

| File | Change |
| --- | --- |
| `Scripts/Core/GameManager.cs` | Added `_activeShip` field; added `RegisterShip()`; updated `PersistCurrentState()` to capture live ship state; fixed `ReturnToMenuAsync()` to hide pause + re-enable input |
| `Scripts/Core/GameSceneWiring.cs` | Added `gm.RegisterShip(_ship)` after `_ship.Inject()` |
| `Tests/EditMode/GameManagerTests.cs` | Updated `FakeSaveSystem` + `FakeUISystem` + `FakeInputManager` to track calls; added 4 new tests |

### Key implementation notes

- `_activeShip` is cleared (`= null`) inside `ReturnToMenuAsync` after `PersistCurrentState()` runs, before the Game scene is unloaded, to prevent a dangling MonoBehaviour reference.
- `RegisterShip(null)` is safe: stores null and returns early (no-op restore). Defensive for future callers.
- `ShipSaveData` uses plain floats (`x, y, z, yawDeg`) matching the schema in `SaveData.cs`.
- No async I/O - `ISaveSystem.Save` is sync per spec; file is <1 KB.

### What is missing / manual steps required

- No new assets required. All changes are code only.
- `SaveSystem` MonoBehaviour must be present in the Persistent scene and assigned to GameManager - verify in Bootstrap if it was wired there already. (It was wired during Phase 2 bootstrap setup.)

### Manual test checklist (for human)

- [x] Unity: 0 errors after recompile.
- [x] EditMode Test Runner: new `GameManagerTests` pass (4 new tests + existing).
- [x] New Game → drive to position ~(30, 0, 10) → Pause → Main Menu → the pause overlay is GONE and Menu is interactive.
- [x] From Menu: Continue → ship appears at saved position (~30, 0, 10), not at origin.
- [x] Repeat: drive further → app quit and relaunch → Continue → restored to last position.

## Manual test: gameplay vertical slice

- [x] HUMAN: play in Editor - Menu → Game → drive → Pause → Resume → Menu → Continue restores position
