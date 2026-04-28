# 📄 `docs/systems/input-manager.md`

## Overview

Abstraction between Unity Input System and gameplay. Hides device differences
(touch vs keyboard) behind a small struct.

---

## Responsibilities

- Read raw input via Unity Input System Actions asset
- Normalize to `[-1, +1]` with deadzone
- Expose latest input every frame
- Surface a single-frame "pause pressed" event

---

## Ownership & Lifetime

- Lives in Persistent scene.
- Owned by GameManager.

---

## Input map (`Assets/Settings/InputActions.inputactions`)

| Action     | Type        | Bindings                                |
| ---------- | ----------- | --------------------------------------- |
| `Move`     | Value Vec2  | WASD, on-screen joystick                |
| `Pause`    | Button      | `Esc`, on-screen pause button           |

Touch joystick is provided via Unity's **On-Screen Stick** component, wired in
the Game scene, sending into the same `Move` action.

---

## Public interface

```
struct MovementInput { float Throttle; float Steering; }
MovementInput GetMovementInput();
bool ConsumePausePressed();   // returns true exactly once per press
void SetEnabled(bool enabled); // disabled during scene transitions
```

---

## Normalization rules

- Deadzone radius `0.15` on the Move stick.
- After deadzone, remap magnitude to `[0, 1]`.
- Clamp throttle to `[-1, +1]`; v0.2 may clamp negative throttle to a small
  reverse factor (configurable in ShipController, not here).

---

## Interactions

### Uses
- Unity Input System (`PlayerInput` or direct `InputAction` asset)

### Used by
- ShipController
- GameManager (pause)
- UI Toolkit input (does not need to query InputManager directly)

---

## Constraints

- No gameplay logic.
- No direct references to ship, camera, or UI.
- No allocations in `GetMovementInput()` (called every frame).
