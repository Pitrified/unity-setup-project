# 📄 `docs/systems/save-system.md`

## Overview

Handles **minimal persistence** for v0.1.

---

## Responsibilities

- Save last player state
- Load last player state

---

## Ownership & Lifetime

- Persistent Scene
- Managed by GameManager

---

## Stored Data

- Player position
- Last scene state

---

## Public Interface

- `Save()`
- `Load()`
- `HasSave()`

---

## Storage

- Local device storage only

---

## Interactions

### Uses

- File system / PlayerPrefs (implementation detail)

---

### Used By

- GameManager
- ShipController (data source)

---

## Constraints

- No complex serialization
- No versioning (v0.1)
