# Project Functional & Technical Specification - v0.2

> **Status:** Source of truth. Any feature not here is out of scope.
> **Audience:** Humans + AI agents. Read before generating any code.

---

## 1. Overview

A 3D mobile game where the player controls a ship navigating a sea environment.
This version delivers a **minimal, stable, extensible vertical slice**.

Priorities (in order):

1. Stable framerate on mid-range Android
2. Clean, AI-legible architecture
3. Fast iteration loop
4. Visual polish (lowest)

---

## 2. Core Technical Decisions (Locked)

| Concern        | Decision                                  | Rationale                              |
| -------------- | ----------------------------------------- | -------------------------------------- |
| Engine         | **Unity 6 LTS** (6000.0.x)                | Current LTS, 2-year support window     |
| Render         | **URP**                                   | Best mobile perf/quality tradeoff      |
| Scripting      | **C# / .NET Standard 2.1**                | Default Unity 6 setting                |
| Backend        | **IL2CPP**, ARM64 only                    | Required for Play Store, best perf     |
| Input          | **Unity Input System** (`com.unity.inputsystem`) | Future-proof, abstraction-friendly |
| Movement       | **Kinematic / arcade**                    | Deterministic, no physics instability  |
| Water          | **Custom URP shader (stylized)**          | Avoid simulation cost                  |
| Camera         | **Third-person chase**                    | Mobile-friendly, no free look          |
| Orientation    | **Landscape only (locked)**               | Single layout to design for            |
| Scenes         | **Multi-scene additive + persistent root** | Clean lifetime separation             |
| Save           | **JSON file in `Application.persistentDataPath`** | Inspectable, portable           |
| Build trigger  | **Local script (manual)**                 | No CI in v0.2                          |

Anything not listed here is **not decided**; do not introduce silent defaults.

---

## 3. Target Platform Constraints

**Reference device:** Pixel 7-class (mid-range Android, 2022+).

**Build requirements (Google Play, 2026):**

- `targetSdkVersion` ≥ 35 (Android 15)
- 64-bit (ARM64) required; ARMv7 disabled
- AAB (Android App Bundle) only - APK rejected for new releases

**Performance budgets:** see [performance-budget.md](performance-budget.md).

---

## 4. Application Lifecycle

```
App Launch
  → Boot Scene (init systems, load Persistent additively)
  → Loading Screen
  → Menu Scene
  → Game Scene (additive load over Persistent)
  → Pause / Resume
  → Back to Menu (unload Game)
  → Quit
```

Ownership rules:

- A central [GameManager](systems/game-manager.md) owns all state transitions.
- No scene file owns global state.
- Scenes are **passive**: they expose entry points, GameManager wires them.

---

## 5. Scene Inventory

| Scene        | Loaded as | Lifetime              | Contents (high level)                  |
| ------------ | --------- | --------------------- | -------------------------------------- |
| `Boot`       | Single    | First frame only      | `GameBootstrap` only                   |
| `Persistent` | Additive  | Whole session         | GameManager, Input, Save, Audio, UIRoot |
| `Menu`       | Single    | While in menu         | Main menu UI                           |
| `Game`       | Additive  | While playing         | Ship, sea, island, GameCamera          |

---

## 6. Game Systems (v0.2)

Each system has a dedicated spec under [systems/](systems/).

| System                                            | Purpose                                |
| ------------------------------------------------- | -------------------------------------- |
| [game-bootstrap](systems/game-bootstrap.md)       | Entry point, loads Persistent          |
| [game-manager](systems/game-manager.md)           | State machine, scene coordination      |
| [scene-loader](systems/scene-loader.md)           | Async additive load/unload             |
| [input-manager](systems/input-manager.md)         | Input abstraction                      |
| [ship-controller](systems/ship-controller.md)     | Kinematic player movement              |
| [camera-controller](systems/camera-controller.md) | Third-person chase camera              |
| [save-system](systems/save-system.md)             | JSON persistence                       |
| [audio-manager](systems/audio-manager.md)         | Music + SFX channels                   |
| [ui-system](systems/ui-system.md)                 | UI Toolkit roots, safe area, scaling   |
| [logging](systems/logging.md)                     | Central log + debug overlay            |

---

## 7. Controls (v0.2)

| Action     | Touch                              | Editor (keyboard)        |
| ---------- | ---------------------------------- | ------------------------ |
| Throttle   | Right-side virtual stick (Y axis)  | `W` / `S`                |
| Steering   | Right-side virtual stick (X axis)  | `A` / `D`                |
| Pause      | Top-left button                    | `Esc`                    |

Single virtual stick is sufficient for v0.2; no twin-stick.

---

## 8. UI Surfaces (v0.2)

- Loading screen (cancel-safe, deterministic min duration 0.5 s to avoid flash)
- Main menu: **Play**, **Continue** (if save exists), **Quit**
- Pause overlay: **Resume**, **Return to menu**
- Built with **UI Toolkit** (preferred) or **uGUI** if blockers arise.
- Must respect **safe area** (notches, gesture bars).

---

## 9. Persistence (v0.1 schema)

Stored as `save.json` in `Application.persistentDataPath`:

```json
{
  "version": 1,
  "ship": { "x": 0.0, "y": 0.0, "z": 0.0, "yawDeg": 0.0 },
  "lastSavedUtc": "2026-04-28T12:00:00Z"
}
```

Rules:

- Atomic write: write to `save.json.tmp`, then rename.
- On unreadable/corrupt file: rename to `save.json.bak`, start fresh, log warning.
- `version` field is mandatory; mismatched versions trigger reset (no migration in v0.2).

---

## 10. Error Handling & Fallbacks

| Failure                       | Behavior                                |
| ----------------------------- | --------------------------------------- |
| Scene load fails              | Log error → return to Menu              |
| Save file corrupt             | Backup + reset (see §9)                 |
| Missing component reference   | Log error, system stays in safe state   |
| Input device disconnected     | Use neutral input until reconnected     |

Never swallow exceptions silently. Never crash in normal flow.

---

## 11. Logging & Debugging

- Central `Log` utility wrapping `UnityEngine.Debug` with category tags.
- Stripped to warnings/errors in Release builds.
- Debug overlay (toggle via 3-finger tap or `F1`):
  - FPS, frame time, draw calls
  - Current GameManager state
  - Buttons: reset player, reload scene, skip loading

See [systems/logging.md](systems/logging.md).

---

## 12. Definition of Demo-Ready

The build is demo-ready when **all** of these hold:

- [ ] Cold start to Menu < 5 s on Pixel 7
- [ ] Menu → Game → Menu loop works repeatedly without leaks
- [ ] Ship is controllable via touch
- [ ] Island is reachable
- [ ] Pause → Resume works
- [ ] Pause → Menu unloads Game cleanly
- [ ] Quit + relaunch + Continue restores last position
- [ ] No errors or warnings in logcat under normal play
- [ ] Sustained ≥ 30 FPS for 5 minutes of play (see [performance-budget.md](performance-budget.md))

---

## 13. Milestones

1. **M0** - Repo + Unity project + LFS configured, opens cleanly
2. **M1** - Bootstrap → Menu → empty Game loop runs in editor
3. **M2** - Ship moves, camera follows, water visible
4. **M3** - Save/load works across sessions
5. **M4** - First Android device build runs
6. **M5** - Internal Testing track on Play Console with ≥1 external tester

---

## 14. Out of Scope for v0.2

Multiplayer · Combat · Realistic water physics · Economy/progression · Cloud
save · IAP · Ads · Analytics · Localization · Accessibility settings · Controller
support · Tablets/foldables special layouts.

These are **not** "later"; they are **not allowed to leak** into v0.2 code.
