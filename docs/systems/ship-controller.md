# 📄 `docs/systems/ship-controller.md`

## Overview

Controls **player ship movement and behavior** using a kinematic (arcade) model.

---

## Responsibilities

- Handle player movement input
- Apply translation and rotation
- Expose ship state (speed, direction)

---

## Ownership & Lifetime

- Attached to Ship GameObject (Prefab)
- Exists only in Game Scene

---

## Core Data

- Speed
- Rotation speed
- Current velocity (logical, not physics)
- Movement input values

---

## Movement Model

- Forward thrust
- Y-axis rotation (turning)
- No physics forces
- No buoyancy simulation

---

## Input Handling

- Receives input via InputManager (not directly from Unity input)

---

## Public Interface (Conceptual)

- `SetInput(throttle, steering)`
- `ResetPosition()`

---

## Update Loop

- Read input
- Compute movement
- Apply transform changes

---

## Interactions

### Uses

- InputManager (indirectly)
- Transform component

---

### Used By

- CameraController (reads position)
- SaveSystem (reads position)

---

## Constraints

- No direct input polling
- No camera logic
- No persistence logic

---

## Failure Handling

- Clamp invalid inputs
- Reset to safe state if needed
