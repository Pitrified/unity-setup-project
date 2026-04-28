# 📄 `docs/systems/scene-loader.md`

## Overview

Wraps `SceneManager` async APIs. Owned by GameManager. Single point that
touches scene loading.

---

## Responsibilities

- Load scenes (single or additive) asynchronously
- Unload scenes safely
- Track active scenes
- Surface loading state to UI

---

## Ownership & Lifetime

- Lives in Persistent scene.
- Owned by GameManager.

---

## Scene IDs

Use a `static class SceneNames` with constants - never raw strings:

```
Boot, Persistent, Menu, Game
```

---

## Public interface

```
Task LoadMenuAsync(CancellationToken ct = default)
Task LoadGameAsync(CancellationToken ct = default)
Task UnloadGameAsync(CancellationToken ct = default)
Task ReloadGameAsync(CancellationToken ct = default)
bool IsLoading { get; }
event Action<float> Progress;     // 0..1
```

---

## Load strategy

| Transition    | Steps                                                   |
| ------------- | ------------------------------------------------------- |
| → Menu        | Unload Game (if loaded) → Load Menu (Single sub-call but Persistent kept via additive trick) |
| → Game        | Load Game additively. Set Game as active scene.         |
| → Reload Game | Unload Game → Load Game.                                |

Persistent is always loaded; never unload it.

---

## Transition behavior

- Block input for the duration (call `InputManager.SetEnabled(false)`).
- Show loading UI via `UISystem.ShowLoading()`.
- Enforce a minimum visible duration of `0.5 s` to avoid flicker.

---

## Interactions

### Uses
- `SceneManager` async APIs
- InputManager (disable/enable)
- UISystem (loading screen)

### Used by
- GameManager (only)

---

## Constraints

- No gameplay logic.
- Idempotent: calling `LoadMenuAsync` while already on Menu is a no-op.
- Concurrent calls return the in-flight Task instead of starting another load.

---

## Failure handling

- Failed load → log error, raise `Progress(1)`, transition to Menu.
- Cancellation → unload anything partially loaded, return to previous state.
