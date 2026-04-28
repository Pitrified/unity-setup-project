# 📄 `docs/systems/camera-controller.md`

## Overview

Third-person chase camera. Follows the ship from a fixed offset with smoothing.

---

## Responsibilities

- Follow target Transform
- Maintain a configurable offset in target-local space
- Smooth position and rotation
- Optional look-at on the target

---

## Ownership & Lifetime

- Component on the Game scene's `MainCamera` GameObject.
- Created/destroyed with the Game scene.

---

## Tunable data (serialized)

- `Vector3 LocalOffset` (default `(0, 3, -6)`)
- `float PositionLerp` (per-second, e.g. `5`)
- `float RotationLerp` (per-second, e.g. `5`)
- `bool LookAtTarget` (default `true`)

Overridable via a `SO_CameraTuning` ScriptableObject in `Assets/Settings/`.

---

## Public interface

```
void SetTarget(Transform target)
void SnapToTarget()              // teleport, used after scene load
```

---

## Update rules

- Runs in `LateUpdate` (after ship moves).
- Interpolation uses `1 - Mathf.Exp(-k * dt)` for framerate-independent smoothing.
- Caches `transform` once.

---

## Interactions

### Uses
- Target `Transform` (typically ShipController's transform)

### Used by
- (Nothing - pure consumer)

---

## Constraints

- No input handling.
- No collision avoidance (v0.2).
- No camera shake / FX in v0.2.

---

## Failure handling

- Null target → camera holds its last position, logs warning once per scene load.
