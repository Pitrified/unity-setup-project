# 📄 `docs/systems/game-manager.md`

## Overview

Central authority over **application state, lifecycle, and high-level
coordination**. Owns references to all systems.

---

## Responsibilities

- Own the global game-state machine
- Coordinate scene transitions via SceneLoader
- Initialize/teardown core systems
- Provide access to shared services (Save, Audio, Input, UI)
- Handle app-level events (focus, pause, quit)

---

## Ownership & Lifetime

- Created in Boot scene during `GameBootstrap`.
- Lives in Persistent scene.
- **Controlled singleton**: `GameManager.Instance` for read-only access; never
  set from outside.
- Survives all scene loads.

---

## Game states

```
enum GameState { Boot, Loading, Menu, Playing, Paused }
```

### Allowed transitions

```
Boot     → Loading
Loading  → Menu
Menu     → Loading       (when starting/continuing a game)
Loading  → Playing
Playing  → Paused
Paused   → Playing
Paused   → Loading       (returning to menu)
Playing  → Loading       (returning to menu)
```

Any other transition is rejected with a logged warning.

---

## Core data

- `GameState CurrentState { get; }`
- `GameState PreviousState { get; }`
- References (assigned by Bootstrap):
  - `SceneLoader`
  - `SaveSystem`
  - `InputManager`
  - `AudioManager`
  - `UISystem`

---

## Public interface

```
void OnBootComplete()                 // called by GameBootstrap
Task StartNewGameAsync()
Task ContinueAsync()                  // requires SaveSystem.HasSave
Task ReturnToMenuAsync()
void PauseGame()
void ResumeGame()
void QuitGame()
event Action<GameState, GameState> StateChanged;   // (previous, next)
```

---

## App-level events

- `OnApplicationPause(true)` while `Playing` → auto-PauseGame and Save.
- `OnApplicationQuit` → Save if `Playing` or `Paused`.
- Focus loss on mobile counts as pause.

---

## Interactions

### Uses
- SceneLoader (scene transitions)
- SaveSystem (persistence)
- InputManager (enable/disable, pause polling)
- AudioManager (snapshots on pause)
- UISystem (screen routing)

### Used by
- UI controllers (menu/pause buttons)
- GameBootstrap (handoff)
- Debug overlay (state inspection)

---

## Constraints

- **No gameplay logic.** No ship, camera, or world references.
- **No direct scene API calls.** Everything through SceneLoader.
- Deterministic state machine: each transition is a single method.
- No `Update` work beyond polling pause input.

---

## Failure handling

- Invalid transition → log warning, no-op.
- Scene load failure (from SceneLoader) → fall back to Menu.
- Save failure on quit → log error, do not block quit.
