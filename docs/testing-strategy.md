# Testing Strategy

Tests are **proportional to value**: most code is glue and gets covered by a
few high-value smoke tests, not a per-class test farm.

---

## 1. Test types

| Type        | Folder                  | Asmdef                      | Speed     | When             |
| ----------- | ----------------------- | --------------------------- | --------- | ---------------- |
| EditMode    | `Assets/Tests/EditMode/` | `Game.Tests.EditMode`       | < 100 ms  | Every commit     |
| PlayMode    | `Assets/Tests/PlayMode/` | `Game.Tests.PlayMode`       | seconds   | Pre-merge / pre-build |
| Smoke (manual) | n/a                  | n/a                         | minutes   | Pre-release      |

Framework: **Unity Test Framework** (NUnit-based). No third-party test deps in v0.2.

---

## 2. What gets EditMode tests

Required:

- Save serialization round-trip (write → read → equality)
- GameManager state machine: every legal transition + every illegal transition rejected
- Input normalization (deadzones, clamping)
- Any pure C# helper with branches

Not required:

- Trivial getters
- MonoBehaviour wiring with no logic

---

## 3. What gets PlayMode tests

Required:

- Bootstrap → Persistent loaded → Menu reachable
- Menu → Game → Menu loop, no leaked GameObjects
- Pause → Resume preserves ship transform
- Save → reload scene → Continue restores position

PlayMode tests load real scenes and run for a few frames. Keep them
deterministic: fixed `Time.captureDeltaTime`, no real-time waits.

---

## 4. Smoke checklist (manual, pre-release)

Run on a physical device:

- [ ] Cold start to Menu < 5 s
- [ ] One full demo loop (Menu → Game → island reached → Pause → Menu)
- [ ] App backgrounded 30 s, foregrounded - resumes cleanly
- [ ] Airplane mode on - app behaves identically (no network in v0.2)
- [ ] Force-quit + relaunch + Continue - position restored
- [ ] Profiler trace: meets [performance-budget.md](performance-budget.md)

---

## 5. Test design rules

- **Arrange / Act / Assert** structure, blank-line separated.
- One logical assertion per test. Use `Assert.Multiple` if grouping is genuine.
- No shared mutable state between tests (`[SetUp]` / `[TearDown]` per fixture).
- Test names: `Method_State_ExpectedResult` - e.g. `Save_WhenFileMissing_StartsFresh`.
- No tests against `Time.deltaTime` real-time; inject a clock or use `UnityEngine.TestTools.Utils`.

---

## 6. Running tests

In Editor: **Window → General → Test Runner**.

Headless (CI-ready):

```
unity -batchmode -nographics -projectPath . \
      -runTests -testPlatform EditMode \
      -testResults Tools/test-results/editmode.xml \
      -logFile Tools/test-results/editmode.log
```

Same with `-testPlatform PlayMode`. Exit code is non-zero on any failure.

---

## 7. Coverage

- Coverage tool: built-in **Code Coverage** package (Window → Analysis → Code Coverage).
- Target: ≥ 70 % line coverage on `Game.Systems` and `Game.Core` asmdefs.
- Coverage is a **smoke detector**, not a goal. Don't write tests for coverage's sake.

---

## 8. AI and tests

When the AI generates a public method, it must also generate at least one test
in the matching test asmdef. Diffs without tests for non-trivial logic are
rejected on review.