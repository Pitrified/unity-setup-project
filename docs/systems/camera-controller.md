# 📄 `docs/systems/camera-controller.md`

## Overview

Implements a **third-person chase camera** following the ship.

---

## Responsibilities

* Follow player ship
* Maintain offset and rotation
* Smooth movement

---

## Ownership & Lifetime

* Exists in Game Scene
* References ShipController

---

## Core Data

* Target (ship transform)
* Offset vector
* Follow speed
* Rotation smoothing

---

## Behavior

* Position = target + offset
* Smooth interpolation
* Optional look-at target

---

## Public Interface

* `SetTarget(Transform)`
* `ResetCamera()`

---

## Interactions

### Uses

* ShipController (position reference)

---

### Used By

* None (pure follower)

---

## Constraints

* No input handling (v0.1)
* No collision handling (v0.1)
