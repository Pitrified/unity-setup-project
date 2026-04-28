# Coding Standards

Lightweight C# rules. Enforced by `.editorconfig` + Unity warnings + reviewer.

---

## 1. Style

- C# language version: default for Unity 6 LTS (currently C# 9, do not enable preview features).
- Indentation: 4 spaces, no tabs.
- Newlines: LF (enforced via `.gitattributes`).
- Trailing whitespace forbidden; file ends with single newline.
- Braces on **new lines** (Allman) - matches Unity sample style.
- `using` directives **inside** the namespace, sorted, no unused.

A starter `.editorconfig` is in the repo root and is the source of truth when
this doc disagrees with it.

---

## 2. Naming

| Construct           | Convention            | Example          |
| ------------------- | --------------------- | ---------------- |
| Types, methods      | `PascalCase`          | `ShipController` |
| Public fields/props | `PascalCase`          | `MaxSpeed`       |
| Private fields      | `_camelCase`          | `_velocity`      |
| Locals, params      | `camelCase`           | `throttleInput`  |
| Constants           | `PascalCase`          | `MaxYawRateDeg`  |
| Interfaces          | `IPascalCase`         | `ISaveStorage`   |
| Async methods       | suffix `Async`        | `LoadAsync`      |

---

## 3. Language idioms

- Prefer `var` only when the type is obvious from the right-hand side.
- Prefer `readonly` and `init` setters where applicable.
- Use `nameof(x)` instead of string literals.
- Use `Span<T>` / `Memory<T>` only when there is a measured perf reason.
- LINQ in hot paths is forbidden; loops in hot paths are required.

---

## 4. Async / coroutines

- `async Task` for I/O and scene loading. No `async void` except event handlers.
- `IEnumerator` coroutines only for time-driven UI (fades, delays).
- Always pass a `CancellationToken` to `async` methods that can outlive the caller.
- Never `.Result` or `.Wait()`. Never block the main thread.

---

## 5. Unity-specific

- `[SerializeField] private` for editor-exposed fields. No public mutable fields.
- `Awake` for self-init only. `Start` for inter-component wiring.
- Cache `Transform` and `Camera.main` once; do not access per frame.
- No allocations in `Update` (no `new`, no LINQ, no boxing).
- Use `Time.deltaTime` (or `unscaledDeltaTime` in pause UI). Never `Time.fixedDeltaTime` outside `FixedUpdate`.

---

## 6. Errors

- Throw `ArgumentException` family for caller bugs at API boundaries.
- For runtime failures (file missing, scene load fail), log + return a result, do not throw.
- Custom exceptions only when caller needs to distinguish - otherwise use built-ins.

---

## 7. Logging

Use the project `Log` utility (see [systems/logging.md](systems/logging.md)).

```
Log.Info(LogCat.Save, "Saved {Path}", path);
Log.Warn(LogCat.Save, "Save corrupt, resetting");
```

Never call `Debug.Log` directly outside the `Log` utility.

---

## 8. Comments

- `///` summary on every public type and member.
- Inline comments explain **why**, never **what** the code already says.
- `// TODO(name): ...` only with an owner. Tracked in `tracking.md` if non-trivial.

---

## 9. Tests

See [testing-strategy.md](testing-strategy.md). Every public method that has
non-trivial logic gets at least one EditMode test.