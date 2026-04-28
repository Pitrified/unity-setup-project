# 📄 `docs/systems/game-bootstrap.md`

## Overview

Single entry point for the application. Lives only in the **Boot scene** and
runs once.

---

## Responsibilities

- Validate runtime environment (target framerate, screen orientation, quality level)
- Load the **Persistent** scene additively
- Hand control to `GameManager` and unload the Boot scene

---

## Ownership & Lifetime

- Single MonoBehaviour `GameBootstrap` on a single GameObject in `Boot.unity`.
- Self-destroys after handoff (Boot scene is unloaded).

---

## Flow

```
App start
 → Apply runtime defaults
     • Application.targetFrameRate = 60
     • Screen.orientation = LandscapeLeft (allowed: LandscapeRight)
     • QualitySettings level = Mobile
 → SceneManager.LoadSceneAsync("Persistent", Additive)
 → Wait until Persistent loaded
 → GameManager.Instance.OnBootComplete()
 → SceneManager.UnloadSceneAsync("Boot")
```

---

## Failure handling

- Persistent fails to load → show non-Unity error UI (plain `OnGUI` is the
  one acceptable use), log fatal, do not retry.
- GameManager not present after Persistent load → fatal, same handling.

---

## Interactions

### Uses
- `SceneManager`
- `GameManager` (one-shot handoff call)

### Used by
- Unity runtime (scene auto-loads)

---

## Constraints

- No gameplay, UI, audio, or save logic.
- No coroutines that outlive the Boot scene.
- Must complete in ≤ 200 ms on Pixel 7.
