# Project Structure & Conventions

## 1. Overview

Defines a **clean, scalable Unity project layout**.

---

## 2. Folder Structure

```
Assets/
  Scripts/
    Core/
    Gameplay/
    Systems/
  Scenes/
  Prefabs/
  Materials/
  UI/
```

---

## 3. Naming Conventions

### Scripts

- PascalCase
- Suffix by role:
  - `Controller`
  - `Manager`
  - `System`

---

### GameObjects

- Clear, descriptive names
- No “GameObject (1)”

---

## 4. Scene Rules

Each scene has a clear purpose:

- Boot → init
- Menu → UI
- Game → gameplay

---

## 5. Prefab Rules

- Reusable objects only
- No scene-specific logic inside prefabs

---

## 6. Script Design Principles

- Single responsibility
- Small components
- No god classes (except GameManager, controlled)

---

## 7. Dependency Rules

- Systems should not depend on scenes
- Gameplay depends on systems, not vice versa

---

## 8. AI Compatibility Rules

- Keep files small and readable
- Avoid hidden logic
- Prefer explicit configuration
