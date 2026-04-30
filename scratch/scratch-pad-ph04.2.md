# Scratch Pad - artificial-pi

Useful commands accumulated during scaffolding. Will be coalesced into the
appropriate docs later.

---

## Phase 4

### Phase 4.2 - Save System

Files created:
- `Assets/Scripts/Systems/Save/SaveData.cs` - `ShipSaveData` + `SaveData` structs (`[System.Serializable]`)
- `Assets/Scripts/Systems/Save/ISaveSystem.cs` - `ISaveSystem` interface
- `Assets/Scripts/Systems/Save/SaveSystem.cs` - `MonoBehaviour` implementation; uses `JsonUtility` + `File.Replace` for atomic writes
- `Assets/Tests/EditMode/SaveSystemTests.cs` - 14 NUnit EditMode tests

Key decisions:
- `SaveData` and `ShipSaveData` are `struct` (value semantics matches spec wording).
- Serialization via `JsonUtility` (built-in; no external packages).
- Atomic write: `WriteAllText` to `save.json.tmp`, then `File.Replace` (or `File.Move` on first save since `File.Replace` requires destination to exist).
- Corrupt or version-mismatch: backup to `save.json.bak`, log warning, return defaults.
- `Save()` stamps `version` and `lastSavedUtc` internally; caller provides ship data.
- `SetSaveDirectory(string)` is `internal` - used only by EditMode tests to point at a temp folder.
- All `.meta` files created with pre-generated GUIDs so Unity imports cleanly.
