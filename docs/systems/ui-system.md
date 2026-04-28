# 📄 `docs/systems/ui-system.md`

## Overview

UI infrastructure shared across scenes: a single root canvas/document, safe-area
handling, and screen routing.

---

## Responsibilities

- Provide a persistent UI root (so loading screen survives scene swaps)
- Apply safe-area insets to all panels
- Route between screens (Loading, Menu, Pause)
- Provide a uniform way to show/hide modal overlays

---

## Ownership & Lifetime

- Lives in Persistent scene as `UIRoot`.
- Owned by GameManager.

---

## Technology choice

- **UI Toolkit** (`UIDocument` + `PanelSettings`) preferred.
- uGUI permitted only for v0.2 fallbacks (e.g. virtual joystick) where UI Toolkit is awkward.
- Mixing both in a single screen is forbidden.

---

## Layout rules

- One `PanelSettings` asset, mobile-tuned (reference resolution 1920×1080
  landscape, scale-with-screen).
- All root containers anchor to safe area; insets fetched from
  `Screen.safeArea` once per resolution change.
- Touch targets ≥ 48 dp on shortest side.
- No text below 14 sp (~28 px on Pixel 7).

---

## Public interface (conceptual)

- `ShowLoading()` / `HideLoading()`
- `ShowScreen(ScreenId id)` - Menu, Pause
- `IsAnyOverlayOpen { get; }`

---

## Screens (v0.2)

| Screen   | Source                | Lives in     |
| -------- | --------------------- | ------------ |
| Loading  | `Loading.uxml`        | Persistent   |
| Menu     | `Menu.uxml`           | Menu scene   |
| Pause    | `Pause.uxml`          | Persistent (overlay) |

---

## Interactions

### Uses

- UI Toolkit
- InputManager (to know when pause is requested)

### Used by

- GameManager (state-driven screen routing)
- SceneLoader (to show loading screen during transitions)

---

## Constraints

- No gameplay logic in UI scripts; UI talks to GameManager only.
- No direct scene loading from button handlers; emit intents.
- No real-time data binding in v0.2 (manual refresh on state change is fine).

---

## Failure handling

- Missing UXML/USS → log error, fall back to a plain text overlay so the user is not stranded on a blank screen.