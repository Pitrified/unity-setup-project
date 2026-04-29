# Scratch Pad - artificial-pi

Useful commands accumulated during scaffolding. Will be coalesced into the
appropriate docs later.

---

## Phase 4

### Phase 4 - Point 1: Logging system (`systems/logging.md`)

**Files created:**

| File | Purpose |
|------|---------|
| `Assets/Scripts/Systems/Logging/LogCat.cs` | `LogCat` enum (Boot, Scene, Input, Save, Audio, UI, Gameplay, Build) |
| `Assets/Scripts/Systems/Logging/Log.cs` | Static facade: `Verbose`, `Info`, `Warn`, `Error`. Ring buffer (20 entries). |
| `Assets/Scripts/Systems/Logging/DebugOverlayController.cs` | MonoBehaviour driving the overlay UIDocument |
| `Assets/UI/DebugOverlay.uxml` | Overlay layout (FPS, frame time, state, log lines, 3 buttons) |
| `Assets/UI/DebugOverlay.uss` | Overlay styles |
| `Assets/Tests/EditMode/LogTests.cs` | 8 EditMode tests covering ring buffer and format args |

**Key design notes:**

- `Verbose` / `Info` are decorated with `[Conditional("UNITY_EDITOR")]` and
  `[Conditional("DEVELOPMENT_BUILD")]` - call sites are stripped by IL2CPP in Release.
- `Warn` / `Error` always compile. Their `params object[]` incurs a string alloc per call,
  which is acceptable since they are not called in hot Update loops.
- Ring buffer write (`Append`) is guarded by `#if UNITY_EDITOR || DEVELOPMENT_BUILD`,
  so it is dead code in Release. Capacity = 20 entries.
- `DebugOverlayController` class body is wrapped in `#if UNITY_EDITOR || DEVELOPMENT_BUILD`.
  In Release the class exists but has no fields/methods; IL2CPP strips it.
- FPS tracking in `DebugOverlayController.Update` is allocation-free (pre-allocated
  `float[60]` circular buffer). UI labels are refreshed at 10 Hz via a timer.
- `Log.GetGameState` (Func<string>) and `Log.OnDebugReset/Reload/SkipLoading` (Actions)
  are hooks; GameManager and SceneLoader bind to them at runtime.
- `Log.ResetForTesting()` is public but guarded by `#if UNITY_EDITOR || DEVELOPMENT_BUILD`.
  It is used in `[SetUp]` of LogTests to isolate test state.

**Manual wiring needed in Unity Editor (Phase 5):**

1. Create a `DebugOverlay` GameObject in `Persistent.unity`.
2. Add UIDocument component; assign `Assets/UI/DebugOverlay.uxml` as Visual Tree Asset.
3. Assign (or create) a PanelSettings asset with a high sort order (e.g. 100) so the
   overlay draws on top of game UI.
4. Add `DebugOverlayController` MonoBehaviour to the same GameObject.
5. In Release Player Settings, ensure `DEVELOPMENT_BUILD` is NOT defined (it is
   not by default for Release builds).

**How to verify in Editor:**

```
# Open Persistent.unity, press Play, press F1 -> overlay should appear/disappear.
# Check Console: no errors. Overlay shows "State: ?" until GameManager is wired.
```

**Run tests:**

```
Window > General > Test Runner > EditMode > Game.Tests.EditMode.LogTests > Run All
```

Expected: 9 tests green.

---

### Phase 4 - Point 1 (addendum): Manual wiring review + fixes (2026-04-30)

#### Test failures (fixed)

Three tests were failing with `"Unhandled log message ... Use UnityEngine.TestTools.LogAssert.Expect"`.
Unity's Test Runner treats any unexpected `Debug.LogError` call as a test failure.

Fix applied to `Assets/Tests/EditMode/LogTests.cs`:
- Added `using UnityEngine;` and `using UnityEngine.TestTools;`
- Added `LogAssert.Expect(LogType.Error, "<exact message>")` immediately before every
  `Log.Error(...)` call in the three affected tests:
  - `Error_WritesEntry_ToRingBuffer`
  - `GetRecentEntries_ReturnsOldestFirst`
  - `ResetForTesting_ClearsBuffer`

Rule to remember: `Log.Warn` does NOT need `LogAssert.Expect` (Unity ignores
`Debug.LogWarning` by default in tests). Only `Log.Error` / `Log.Exception` do.

The scratch pad previously said "8 tests green" - corrected above to 9.

#### Scene: SampleScene vs Persistent.unity

`Persistent.unity` does not exist yet (Phase 5 task). The DebugOverlay
GameObject was wired into `SampleScene.unity` for now - that is fine as a
temporary test bed.

**Action required in Phase 5** when `Persistent.unity` is created:
1. Open `SampleScene.unity` in the Editor.
2. Move (cut/paste) the `DebugOverlay` GameObject into `Persistent.unity`.
3. Delete it from `SampleScene.unity`.
4. Commit both scene files.

#### PanelSettings asset: rename and relocate

Currently: `Assets/UI/NewPanelSettings.asset`

- Wrong folder: UI assets (`Assets/UI/`) hold UXML/USS only; config assets go in
  `Assets/Settings/`.
- Wrong name: ScriptableObject instances follow the `SO_<purpose>` convention
  per `docs/project-structure.md §4`.

**Action required (must be done inside Unity Editor - do not rename/move on disk):**
1. In the Project window, drag `Assets/UI/NewPanelSettings.asset` to
   `Assets/Settings/`.
2. Right-click it -> Rename -> `SO_DebugOverlayPanel`.
3. Unity will update the .meta and any scene references automatically.
4. Verify `SampleScene.unity` still has the PanelSettings reference on the
   UIDocument component (should be automatic).
5. Commit `Assets/Settings/SO_DebugOverlayPanel.asset` + its `.meta` and the
   updated `Assets/UI/` (removed file + `.meta`).

#### Wiring verification (2026-04-30 - SampleScene)

Checks against the scene YAML:

| Item | Expected | Result |
|------|----------|--------|
| `DebugOverlay` GameObject exists | yes | PASS |
| `DebugOverlayController` component attached | yes | PASS |
| `UIDocument` component attached | yes | PASS |
| UXML sourceAsset GUID matches `DebugOverlay.uxml` | `3316a3d865dc72b069d69270852d0d79` | PASS |
| PanelSettings GUID matches `NewPanelSettings.asset` | `da1ac7cd0e711581a8e4fa7e212dba03` | PASS |
| PanelSettings sort order | 100 | PASS (set in asset) |

One known gap: the UIDocument component's own `m_SortingOrder` field is 0 (the
default offset within the panel). This is correct - the sort order that matters
for overlay stacking is the one on the PanelSettings asset (100).

