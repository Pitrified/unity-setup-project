# 📄 `docs/systems/avatar-controller.md`

## Overview

The sample's placeholder avatar, a capsule, using a **kinematic** model - the Transform is moved directly,
no Rigidbody forces. Arcade tank controls: forward/back and turn.
It began as artificial-pirate's ship controller, whose tuned version is linked from the root README.

---

## Responsibilities

- Read movement input from InputManager
- Apply forward speed along the avatar's forward
- Apply yaw rotation from steering
- Expose current avatar state for the camera and save system

---

## Ownership & Lifetime

- Component on the `PF_Avatar` prefab.
- Lives only inside the Game scene.

---

## Tunable data (serialized → `SO_AvatarTuning`)

| Field            | Default | Notes                                       |
| ---------------- | ------- | ------------------------------------------- |
| `MaxSpeed`       | 8 m/s   |                                             |
| `Acceleration`   | 4 m/s²  | Approach `MaxSpeed` from current speed      |
| `ReverseFactor`  | 0.3     | Multiplier on `MaxSpeed` for negative throttle |
| `YawRateDeg`     | 60 °/s  | At full steering input                      |
| `SteeringResponseLerp` | 6 | Per-second smoothing of steering input    |

---

## Movement model

- `currentSpeed` lerps toward `MaxSpeed * throttle` at `Acceleration`.
- `transform.position += transform.forward * currentSpeed * Time.deltaTime`.
- `transform.Rotate(Vector3.up, YawRateDeg * smoothedSteering * Time.deltaTime)`.
- Y is locked at `_groundY` (1, half the capsule's height, so it stands on the plane at y 0).

No Rigidbody, no `MovePosition`, no physics queries.

---

## Public interface

```
void SetInput(float throttle, float steering)   // [-1, 1] each
void Teleport(Vector3 position, float yawDeg)   // used by GameManager on restore
AvatarState GetState()                          // position + yaw
```

---

## Interactions

### Uses
- `InputManager.GetMovementInput()` (inside `Update`)

### Used by
- CameraController (reads transform)
- GameManager (reads `GetState` on save, calls `Teleport` on restore)

---

## Constraints

- No collision handling.
- No allocations per frame.
- No camera, audio, or UI calls.

---

## Failure handling

- NaN/Infinity in input → clamp to 0 and log warning.
- Position drifts beyond ±10 km from origin → warning logged, no auto-action.
