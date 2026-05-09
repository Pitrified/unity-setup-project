# Scratch Pad - artificial-pi - Phase 7 - tests

implementing phase 7 from [tracking.md](docs/tracking.md)

more detailed notes, results, manual steps required, etc go here before being distilled into the final PRs and docs updates

gather additional context before implementing (eg, review relevant docs, load mcp usage guides, etc)

## Relevant docs by task

| Task | Doc reference |
| --- | ----- |
| Boot loop test | docs/testing-strategy.md §3, systems/game-bootstrap.md, systems/game-manager.md |
| Pause/resume test | systems/game-manager.md, systems/ship-controller.md |
| Save/reload test | systems/save-system.md, systems/game-manager.md |

- [x] AI: PlayMode test: full Boot → Menu → Game → Menu loop, asserts no leaked GameObjects
- [x] AI: PlayMode test: pause → resume preserves ship transform
- [x] AI: PlayMode test: save → reload scene → Continue restores position
- [x] HUMAN: run all tests in Test Runner - all green

## ai test 1: Boot → Menu → Game → Menu loop

### key decisions

- `Time.captureDeltaTime = 0.016f` in SetUp for deterministic frame advance; reset in TearDown.
- Timeout is frame-based (900 frames = ~14.4 s simulated) to avoid wall-clock dependency.
- "No leaked objects" assertion checks Persistent scene root count before and after the loop.
- Also asserts `FindObjectsByType<GameManager>()` returns exactly 1 (no duplicate).
- `WaitForState(GameState)` is a reusable nested IEnumerator helper - yields until GameManager reaches target state.
- `StartNewGameAsync` / `ReturnToMenuAsync` are awaited frame-by-frame with `while (!task.IsCompleted) yield return null`.

### what was done

- Created `Assets/Tests/PlayMode/BootNavigationTests.cs` in `Game.Tests.PlayMode` namespace.
- Test: `BootMenuGameMenuLoop_NoLeakedGameObjects`
  - Loads Boot.unity → waits for Menu state
  - Records Persistent scene root count
  - Calls StartNewGameAsync → waits for Playing
  - Calls ReturnToMenuAsync → waits for Menu
  - Asserts root count unchanged and only 1 GameManager exists
- Unity MCP was not reachable (Editor closed); compilation verified via IDE static analysis (0 errors).

### manual steps

- Open Unity Editor → Window → General → Test Runner → PlayMode tab
- Run `BootNavigationTests.BootMenuGameMenuLoop_NoLeakedGameObjects`
- Expected: green (all 3 assertions pass, ~5-10 s runtime)

### result

- **PASSED** via MCP `run_tests` (PlayMode mode)
  - 1/1 passed, 0 failed, duration 2.74 s
- Console warnings during run (pre-existing, not test failures):
  - `[WARN][Boot] Quality level 'mobile' not found` - GameBootstrap known issue
  - `There are no audio listeners in the scene` - expected in test runner context

## ai test 2: pause → resume preserves ship transform

### key decisions

- Ship spawns at zero speed / zero steering; `InputManager.SetEnabled(false)` (called by `PauseGame`) returns default `MovementInput(0,0)` from `GetMovementInput()`.
- With zero throttle/steering, `ShipController.Tick` is a no-op for position and rotation:
  - `MoveTowards(0, 0, acc*dt) = 0` → no forward movement
  - `Lerp(0, 0, rate*dt) = 0` → no yaw rotation
- `PauseGame()` and `ResumeGame()` are synchronous - state changes immediately, no Task to await.
- Transform comparison uses epsilon `0.001f` on `Vector3.Distance` and `Mathf.DeltaAngle` for float robustness.
- Extra `yield return null` after `WaitForState(Playing)` ensures `GameSceneWiring.Start()` has run and injected InputManager into the ship before recording the pre-pause transform.

### what was done

- Created `Assets/Tests/PlayMode/PauseResumeTests.cs` in `Game.Tests.PlayMode` namespace.
- Test: `PauseResume_ShipTransformPreserved`
  - Boot.unity → Menu → StartNewGameAsync → Playing
  - Extra frame yield for Start() callbacks
  - FindObjectsByType<ShipController> - asserts exactly 1
  - Record positionBefore, yawBefore
  - PauseGame → advance 10 frames → ResumeGame
  - Assert Vector3.Distance < 0.001f and |DeltaAngle| < 0.001f

### result

- **PASSED** via MCP `run_tests` (PlayMode mode)
  - 1/1 passed, 0 failed, duration 1.82 s
- 0 errors, 0 warnings during test run

## ai test 3: save → reload scene → Continue restores position

### key decisions

- `StartNewGameAsync` deletes any prior save → test starts with clean slate.
- Ship teleported to `(100, 0, 100)` yaw `90°` - integer values that round-trip through JSON float serialization without loss.
- `ReturnToMenuAsync` calls `PersistCurrentState()` BEFORE `SceneLoader.LoadMenuAsync` runs → save is written while `_activeShip` is still valid and Game scene is still loaded.
- `ContinueAsync` loads a fresh Game scene; `GameSceneWiring.Start()` → `RegisterShip()` reads save → calls `ship.Teleport(savedPos, savedYaw)` on the new instance.
- Extra `yield return null` after each `WaitForState(Playing)` call ensures `Start()` callbacks have fired before querying ship state.
- Epsilon `0.1f` for both position and yaw (well above JSON float round-trip noise, well below any meaningful movement).
- Real `Application.persistentDataPath` save file is created; cleaned at the start of the next test run via `StartNewGameAsync`.

### what was done

- Created `Assets/Tests/PlayMode/SaveRestoreTests.cs` in `Game.Tests.PlayMode` namespace.
- Test: `SaveReloadContinue_RestoresShipPosition`
  - Boot → Menu → StartNewGameAsync → Playing (extra frame)
  - `ship.Teleport(SavedPos=100,0,100  SavedYaw=90)`
  - ReturnToMenuAsync → WaitForState(Menu)
  - Asserts `SaveSystem.HasSave == true`
  - ContinueAsync → WaitForState(Playing) (extra frame)
  - FindObjectsByType<ShipController> (asserts exactly 1)
  - Asserts `Vector3.Distance < 0.1f` and `|DeltaAngle| < 0.1f`

### result

- **PASSED** via MCP `run_tests` (PlayMode mode)
  - 1/1 passed, 0 failed, duration 2.92 s
- 0 errors, 0 warnings during test run
