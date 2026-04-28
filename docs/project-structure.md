# Project Structure & Conventions

## 1. Goal

A **flat, predictable Unity layout** that is easy for both humans and AI agents
to navigate. Every file has one obvious place.

---

## 2. Top-level layout

```
<repo>/
├── Assets/
│   ├── Art/                  # Models, textures, materials (source-of-truth)
│   ├── Audio/                # Music, SFX
│   ├── Prefabs/
│   ├── Scenes/
│   │   ├── Boot.unity
│   │   ├── Persistent.unity
│   │   ├── Menu.unity
│   │   └── Game.unity
│   ├── Scripts/
│   │   ├── Core/             # Bootstrap, GameManager, SceneLoader
│   │   ├── Systems/          # Input, Save, Audio, Logging
│   │   ├── Gameplay/         # Ship, Camera
│   │   ├── UI/               # UI controllers, view models
│   │   └── Editor/           # Editor-only tools (own .asmdef)
│   ├── Settings/             # URP assets, Input Actions, Quality settings
│   ├── Shaders/              # Custom shaders / shader graphs
│   ├── UI/                   # UXML, USS, sprites
│   └── Tests/
│       ├── EditMode/
│       └── PlayMode/
├── Packages/                 # manifest.json + lock
├── ProjectSettings/          # All committed
├── Tools/                    # Build scripts, repo scripts
├── docs/                     # This folder
├── .editorconfig
├── .gitignore
├── .gitattributes            # LFS rules - see git-workflow.md
└── README.md
```

`Assets/StreamingAssets/`, `Assets/Resources/` are **not used** by default. Add
only with explicit justification (both have build-size and load-time
implications).

---

## 3. Assembly Definitions (`.asmdef`)

Required to keep build times low and dependencies explicit:

| Folder                    | Asmdef name              | References               |
| ------------------------- | ------------------------ | ------------------------ |
| `Scripts/Core/`           | `Game.Core`              | Systems, Gameplay, UI    |
| `Scripts/Systems/`        | `Game.Systems`           | (none, leaf)             |
| `Scripts/Gameplay/`       | `Game.Gameplay`          | `Game.Systems`           |
| `Scripts/UI/`             | `Game.UI`                | `Game.Systems`           |
| `Scripts/Editor/`         | `Game.Editor` (Editor)   | All above                |
| `Tests/EditMode/`         | `Game.Tests.EditMode`    | All non-editor           |
| `Tests/PlayMode/`         | `Game.Tests.PlayMode`    | All non-editor           |

Dependency direction is **one-way**: `Core → {Systems, Gameplay, UI}`. Systems
never depend on Gameplay or UI.

---

## 4. Naming conventions

**Scripts (C#):**

- `PascalCase` filename matches the public type inside.
- One public type per file.
- Suffix by role: `Controller`, `Manager`, `System`, `View`, `Settings`, `Data`.
- Private fields: `_camelCase`. Serialized private fields: `[SerializeField] private T _name;`.

**GameObjects:** Clear descriptive names (`Ship`, `MainCamera`, `UIRoot`).
Never leave `GameObject (1)`.

**Assets:**

- Prefabs: `PF_Ship`, `PF_Island`
- Materials: `M_Water`, `M_Hull`
- Textures: `T_Hull_Albedo`, `T_Hull_Normal`
- Scenes: bare name (`Game.unity`)
- Input Actions: `InputActions.inputactions`
- ScriptableObject instances: `SO_<purpose>`

---

## 5. Scene rules

- Each scene has **one purpose** (see [functional-specs.md §5](functional-specs.md#5-scene-inventory)).
- Cross-scene references are **forbidden**. Use GameManager APIs.
- A scene must be playable from the editor without manual setup, **except**
  Game scene which requires Persistent loaded first (provide editor helper).

---

## 6. Prefab rules

- Prefab variants only when there is a clear hierarchy.
- No scene-specific data baked into prefabs.
- Prefer **prefab + ScriptableObject config** over hard-coded values.

---

## 7. ScriptableObject usage

Use `ScriptableObject` for:

- Static gameplay data (ship stats, camera tuning)
- Cross-system references that should not live in scenes

Do **not** use `ScriptableObject` for runtime mutable state.

---

## 8. Component design

- **Single responsibility** per MonoBehaviour.
- Public surface = serialized fields + a few methods. No public mutable fields.
- Avoid `FindObjectOfType` and `GameObject.Find` outside Bootstrap.
- Prefer constructor-style wiring via `[SerializeField]` references.

---

## 9. Forbidden patterns

- `static` mutable state outside of explicit singletons (`GameManager`, `Log`).
- `OnGUI` (use UI Toolkit / uGUI).
- Coroutines for state machines (use plain async or explicit FSM).
- `Resources.Load` and `Resources/` folder (use direct references or Addressables later).

---

## 10. AI-friendliness rules

- Files ≤ ~300 lines. Split if larger.
- No hidden behavior in `Awake`/`OnEnable` beyond simple wiring.
- Public API documented with one-line `///` summaries.
- Constants over magic numbers; name explains intent.
