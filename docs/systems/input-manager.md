# 📄 `docs/systems/input-manager.md`

## Overview

Abstraction layer between **Unity Input System** and gameplay systems.

---

## Responsibilities

- Read raw input
- Normalize inputs
- Expose input state

---

## Ownership & Lifetime

- Persistent Scene
- Managed by GameManager

---

## Core Data

- Throttle input
- Steering input
- Pause input

---

## Public Interface

- `GetMovementInput()`
- `IsPausePressed()`

---

## Interactions

### Uses

- Unity Input System

---

### Used By

- ShipController
- GameManager (pause handling)

---

## Constraints

- No gameplay logic
- No direct references to ship
