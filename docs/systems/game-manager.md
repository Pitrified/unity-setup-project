# 📄 `docs/systems/game-manager.md`

## Overview

Central authority controlling **application state, lifecycle, and high-level coordination** across scenes.

---

## Responsibilities

- Own global **game state**
- Coordinate **scene transitions**
- Initialize core systems
- Provide access to shared services
- Handle app-level events (pause, resume, quit)

---

## Ownership & Lifetime

- Created in **Boot Scene**
- Lives in **Persistent Scene**
- Implemented as **singleton (controlled)**
- Must persist across scene loads

---

## Game States

Defined states:

- `Boot`
- `Loading`
- `Menu`
- `Playing`
- `Paused`

---

## Core Data

Holds:

- Current game state
- Previous game state
- References to:
  - SceneLoader
  - SaveSystem
  - InputManager
  - AudioManager

---

## Public Interface (Conceptual)

- `StartGame()`
- `ReturnToMenu()`
- `PauseGame()`
- `ResumeGame()`
- `QuitGame()`
- `LoadLastSession()`

---

## State Transitions

```id="gm-flow"
Menu → Playing
Playing → Paused
Paused → Playing
Playing → Menu
```

---

## Interactions

### Uses

- SceneLoader → to load/unload scenes
- SaveSystem → to load/save session
- InputManager → to enable/disable input

---

### Used By

- UI (menu buttons)
- Pause system
- Bootstrap logic

---

## Constraints

- No gameplay logic
- No direct scene manipulation (delegates to SceneLoader)
- Must be deterministic and simple

---

## Failure Handling

- Invalid state transitions must be ignored or logged
- If scene load fails → fallback to Menu
