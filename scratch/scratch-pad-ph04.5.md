# Scratch Pad - artificial-pi

Useful commands accumulated during scaffolding. Will be coalesced into the
appropriate docs later.

---

## Phase 4

### Phase 4.5 - UISystem (loading + pause overlays)

**Files created:**

| File | Purpose |
|------|---------|
| `Assets/Scripts/UI/ScreenId.cs` | `ScreenId` enum (`Menu`, `Pause`) |
| `Assets/Scripts/UI/IUISystem.cs` | Contract: `ShowLoading`, `HideLoading`, `ShowScreen`, `HideScreen`, `IsAnyOverlayOpen`, `OnResumeRequested`, `OnReturnToMenuRequested` |
| `Assets/Scripts/UI/UISystem.cs` | `MonoBehaviour` implementation; two `[SerializeField] UIDocument` fields for loading + pause; safe-area insets applied to `safe-area-container` element on resolution change |
| `Assets/UI/Loading.uxml` | Full-screen black overlay with "Loading..." label |
| `Assets/UI/Loading.uss` | Styles for loading overlay |
| `Assets/UI/Pause.uxml` | Semi-transparent overlay; Resume + Main Menu buttons named `btn-resume` / `btn-return-to-menu` |
| `Assets/UI/Pause.uss` | Styles for pause overlay; buttons >= 96 px tall (>= 48 dp at 2x density) |
| `Assets/Tests/EditMode/UISystemTests.cs` | 11 EditMode tests covering boolean state machine (Start not called in EditMode; null-guard on VisualElement operations is the contract that makes this work) |

**Design notes:**
- `UISystem.Start()` calls `BindDocument()` for each `UIDocument` reference; logs Error and returns `null` root if document or UXML is missing (failure-safe, no blank screen crash).
- `UISystem.Update()` compares `Screen.safeArea` and screen size every frame; re-applies pixel padding to `safe-area-container` only when a change is detected. All comparisons are value-type (no allocations).
- Button events (`OnResumeRequested`, `OnReturnToMenuRequested`) are wired in `Start()`; GameManager subscribes to them (Phase 5 wiring).
- `Game.Tests.EditMode` asmdef already referenced `Game.UI` - no asmdef changes needed.

**Phase 5 wiring tasks (not done here):**
- Add `UIRoot` GameObject to Persistent scene; attach `UISystem` + two child `UIDocument` components.
- Assign `Loading.uxml` / `Loading.uss` (or shared `PanelSettings`) to the loading `UIDocument`.
- Assign `Pause.uxml` / `Pause.uss` to the pause `UIDocument`.
- Create `PanelSettings` asset in `Assets/Settings/` (1920x1080 landscape, Scale With Screen Size).
- `GameManager` subscribes to `ui.OnResumeRequested` and `ui.OnReturnToMenuRequested`.
