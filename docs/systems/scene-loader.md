# 📄 `docs/systems/scene-loader.md`

## Overview

Handles **all scene loading/unloading**, including additive scenes and transitions.

---

## Responsibilities

* Load scenes (single or additive)
* Unload scenes safely
* Track active scenes
* Provide loading state feedback

---

## Ownership & Lifetime

* Instantiated by GameManager
* Lives in Persistent Scene

---

## Core Data

* Active scene list
* Loading state (bool)
* Target scene identifiers

---

## Scene Types

* Boot (initial only)
* Persistent (always loaded)
* Menu
* Game

---

## Public Interface (Conceptual)

* `LoadMenu()`
* `LoadGame()`
* `ReloadGame()`
* `UnloadGame()`
* `IsLoading()`

---

## Loading Strategy

### Menu Load

* Unload Game Scene (if loaded)
* Load Menu Scene

---

### Game Load

* Load Game Scene additively
* Ensure Persistent Scene remains

---

## Transition Behavior

* Trigger loading UI via GameManager
* Block input during transitions

---

## Interactions

### Uses

* Unity SceneManager API

---

### Used By

* GameManager (primary)
* Debug tools (optional)

---

## Constraints

* No game logic
* No UI logic
* Must be idempotent (safe to call multiple times)

---

## Failure Handling

* Failed load → notify GameManager
* Prevent duplicate loads
