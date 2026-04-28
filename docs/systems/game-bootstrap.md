# 📄 `docs/systems/game-bootstrap.md`

## Overview

Initial entry point responsible for **bootstrapping the application**.

---

## Responsibilities

- Initialize GameManager
- Load Persistent Scene
- Start initial flow

---

## Ownership & Lifetime

- Exists in Boot Scene only
- Runs once

---

## Flow

```id="bootstrap-flow"
App Start
→ Initialize systems
→ Load Persistent Scene
→ Transition to Menu
```

---

## Interactions

### Uses

- GameManager
- SceneLoader

---

### Used By

- Unity runtime (entry point)

---

## Constraints

- Minimal logic
- No long-running responsibilities

---

---

# ✅ Result

You now have **clean, decoupled system specs** aligned with:

- AI-driven development
- Mobile constraints
- Scalable architecture
