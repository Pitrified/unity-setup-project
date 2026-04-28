# 📄 `docs/systems/save-system.md`

## Overview

Single-file JSON persistence for the player session. Schema in
[functional-specs.md §9](../functional-specs.md#9-persistence-v01-schema).

---

## Responsibilities

- Read/write `save.json` from `Application.persistentDataPath`
- Validate schema version
- Provide a clean `HasSave` check for the menu's Continue button

---

## Ownership & Lifetime

- Lives in Persistent scene.
- Owned by GameManager.

---

## Public interface

```
bool HasSave { get; }
SaveData Load()                  // returns default if no/corrupt save
void Save(SaveData data)         // atomic
void DeleteSave()                // for debug + future "new game" flow
```

`SaveData` is a plain serializable struct matching the schema.

---

## Storage rules

- File path: `Path.Combine(Application.persistentDataPath, "save.json")`.
- Atomic write: write to `save.json.tmp`, `File.Replace` to `save.json`.
- On corrupt read: rename to `save.json.bak`, log warning, return defaults.
- Version mismatch: same as corrupt (no migration in v0.2).

---

## Serialization

- `System.Text.Json` if available; otherwise `JsonUtility`.
- No reflection-heavy options. No external JSON libraries.
- Floats serialized with invariant culture.

---

## Interactions

### Uses
- File system

### Used by
- GameManager (save on pause/quit, load on Continue)
- ShipController (provides `SaveData.ship` snapshot)

---

## Constraints

- No PlayerPrefs (except audio volumes via AudioManager).
- No async I/O in v0.2 - file is small (<1 KB), sync write is fine.
- No encryption; the file is intentionally inspectable.

---

## Failure handling

- Disk full / write fails → log error, keep in-memory state, do not crash.
- Read fails → see "corrupt" rule above.
