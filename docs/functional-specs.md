# Project Functional & Technical Specification - v0.2

## 1. Overview

A 3D mobile game where the player controls a ship navigating a sea environment.
This version focuses on delivering a **minimal, stable, and extensible vertical slice**.

The project prioritizes:

- Fast iteration
- Clean architecture
- AI-assisted development
- Mobile performance (mid-range devices like Pixel 7)

---

## 2. Core Technical Decisions (Locked)

### Engine & Rendering

- Unity Version: **Latest LTS**
- Render Pipeline: **URP (Universal Render Pipeline)**

Rationale:

- Stability + long-term support
- Best mobile performance tradeoff

---

### Input System

- **Unity Input System (new)**

Rationale:

- Future-proof
- Mobile-friendly
- Supports abstraction layer

---

### Movement Model

- **Kinematic / Arcade movement**

Rationale:

- Full control over gameplay feel
- Avoids physics instability
- Easier iteration

---

### Water Strategy

- **Custom lightweight shader (stylized, non-physical)**

Rationale:

- Avoid heavy simulation
- Maintain performance
- Full control over visuals

---

### Camera System

- **Third-person chase camera**

Constraints:

- Smooth follow
- Limited player control
- Mobile-friendly

---

### Orientation

- **Landscape only (locked)**

---

### Scene Architecture

- **Multi-scene setup with persistent root**

Structure:

- Boot Scene
- Persistent Scene (GameManager, systems)
- Menu Scene
- Game Scene (loaded additively)

---

### Save System (v0.1)

- Minimal persistence:
  - Last player position
  - Last session state

---

### Build Strategy

- Scripted builds
- Manual trigger
- No CI/CD (yet)

---

## 3. Target Platform Constraints

Target device: **mid-range Android (e.g. Pixel 7)**

Constraints:

- Stable framerate > visual fidelity
- Limited draw calls
- No heavy shaders or physics
- Memory-conscious asset usage

---

## 4. Application Lifecycle

Flow:

```
App Launch
  ↓
Boot Scene
  ↓
Loading Screen
  ↓
Main Menu
  ↓
Game Scene
  ↓
Pause / Resume
  ↓
Back to Menu
```

Ownership:

- Central **GameManager** controls state transitions
- No scene should own global logic

---

## 5. Scene Structure

### Boot Scene

- Initializes core systems
- Loads persistent scene

---

### Persistent Scene

Contains:

- GameManager
- Input Manager
- Save System
- Audio Manager

Must survive scene loads.

---

### Menu Scene

- Main menu UI
- Navigation entry point

---

### Game Scene

Contains:

- Ship (player)
- Sea surface
- Island (static)

---

## 6. Game Systems

### 6.1 Player Controller

- Kinematic movement
- Forward thrust + steering
- No physics-based buoyancy

---

### 6.2 Camera System

- Follows ship with offset
- Smooth interpolation
- No free rotation (initially)

---

### 6.3 Water System

- Simple plane mesh
- Shader-based animation
- No real interaction with ship

---

### 6.4 Game State System

States:

- Menu
- Playing
- Paused

Managed centrally.

---

### 6.5 Pause System

- Freeze gameplay
- Show pause UI
- Options:
  - Resume
  - Return to menu

---

## 7. UI System

Initial UI:

- Loading screen
- Main menu
- Pause menu

Constraints:

- Mobile-safe layout
- Resolution-independent

---

## 8. Persistence

Stored locally:

- Last position
- Last state

No:

- Inventory
- World state
- Cloud sync

---

## 9. Error Handling & Fallbacks

Must handle:

- Scene load failure → return to menu
- Missing references → safe defaults

No hard crashes in normal flow.

---

## 10. Logging & Debugging

- Central logging utility
- Debug shortcuts:
  - Reset player position
  - Skip loading
  - Reload scene

Optional debug UI overlay.

---

## 11. Definition of Demo-Ready

The build is considered ready when:

- App launches without errors
- Menu → Game → Menu works
- Ship is controllable
- Island is reachable
- Pause works
- No crashes in normal usage

---

## 12. Milestones

1. Project setup complete
2. First local build running
3. Core architecture implemented
4. Minimal game world playable
5. Private alpha published

---

## 13. Out of Scope for v0.2

- Multiplayer
- Combat
- Realistic water physics
- Economy/progression
