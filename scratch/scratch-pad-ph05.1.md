# Scratch Pad - artificial-pi - Phase 5 - scenes

implementing phase 5 from [tracking.md](docs/tracking.md)

more detailed notes, results, manual steps required, etc go here before being distilled into the final PRs and docs updates

gather additional context before implementing (eg, review relevant docs, load mcp usage guides, etc)

## Relevant docs by task

| Task | Doc reference |
|------|---------------|
| wire `Boot.unity` | [systems/game-bootstrap.md](../docs/systems/game-bootstrap.md) - flow, constraints, single GO rule |
| wire `Persistent.unity` | [systems/game-manager.md](../docs/systems/game-manager.md), [systems/scene-loader.md](../docs/systems/scene-loader.md), [systems/audio-manager.md](../docs/systems/audio-manager.md), [systems/input-manager.md](../docs/systems/input-manager.md), [systems/save-system.md](../docs/systems/save-system.md), [systems/ui-system.md](../docs/systems/ui-system.md) |
| wire `Menu.unity` | [systems/game-manager.md](../docs/systems/game-manager.md) §Public interface - Play/Continue/Quit entry points, [systems/ui-system.md](../docs/systems/ui-system.md) §Screens |
| wire `Game.unity` | [systems/ship-controller.md](../docs/systems/ship-controller.md), [systems/camera-controller.md](../docs/systems/camera-controller.md), [project-structure.md](../docs/project-structure.md) §2 (sea plane, PF_Ship prefab) |
| register scenes in Build Settings | [project-structure.md](../docs/project-structure.md) §2 - scene list: Boot, Persistent, Menu, Game (Boot index 0) |

## Scene Creation

- [x] HUMAN: create `Boot.unity`, `Persistent.unity`, `Menu.unity`, `Game.unity` (commit empty) -- done via MCP 2026-05-03

## wire boot scene

- [x] AI: wire `Boot.unity` (single `GameBootstrap`) -- done via MCP 2026-05-04

### What was implemented (MCP)
- Loaded `Assets/Scenes/Boot.unity` (was empty)
- Created one GameObject named `GameBootstrap` at origin
- Added `Game.Core.GameBootstrap` component (no serialized fields - component is self-contained)
- Saved the scene
- Console verified clean (0 errors, 0 warnings)

### Final Boot.unity hierarchy
```
Boot.unity
└── GameBootstrap  [Transform, GameBootstrap]
```

### Manual steps still to do
- None for this scene. The `GameBootstrap` script handles all boot logic at runtime.
- The Persistent scene must be wired (next task) before Boot→Persistent handoff works.
- Boot.unity must be added to Build Settings at index 0 (handled in the "register scenes" task).

## wire persistent scene

- [x] AI: wire `Persistent.unity` (GameManager, all systems, UIRoot) -- done via MCP 2026-05-04

### What was implemented (MCP)

**New asset created:**
- `Assets/Settings/GamePanelSettings.asset` - PanelSettings (1920x1080, ScaleWithScreenSize, match=0.5, sortOrder=0)

**GameObjects + components:**

| GameObject | Component(s) | Serialized refs wired |
|---|---|---|
| `GameManager` | `Game.Core.GameManager` | none (receives refs via `Initialize()` at runtime) |
| `SceneLoader` | `Game.Core.SceneLoader` | none (receives refs via `Initialize()` at runtime) |
| `SaveSystem` | `Game.Systems.SaveSystem` | none (uses `Application.persistentDataPath`) |
| `InputManager` | `Game.Systems.InputManager` | `_inputActions` -> `Assets/Settings/InputActions.inputactions` |
| `AudioManager` | `Game.Systems.AudioManager` | `_mixer` -> MainMixer, `_musicGroup` -> Music, `_sfxGroup` -> SFX |
| `UIRoot` | `Game.UI.UISystem` | `_loadingDocument` -> Loading child, `_pauseDocument` -> Pause child |
| `UIRoot/Loading` | `UIDocument` | `m_PanelSettings` -> GamePanelSettings, `sourceAsset` -> Loading.uxml |
| `UIRoot/Pause` | `UIDocument` | `m_PanelSettings` -> GamePanelSettings, `sourceAsset` -> Pause.uxml |

All 10 references verified non-null. Console: 0 errors, 0 warnings after save.

### Final Persistent.unity hierarchy
```
Persistent.unity
├── GameManager   [Transform, GameManager]
├── SceneLoader   [Transform, SceneLoader]
├── SaveSystem    [Transform, SaveSystem]
├── InputManager  [Transform, InputManager]
├── AudioManager  [Transform, AudioManager]
└── UIRoot        [Transform, UISystem]
    ├── Loading   [Transform, UIDocument]  <- Loading.uxml + GamePanelSettings
    └── Pause     [Transform, UIDocument]  <- Pause.uxml + GamePanelSettings
```

### Notes
- `GameBootstrap` discovers all systems at runtime via `FindAnyObjectByType<T>()` and calls `GameManager.Initialize(...)`. No extra wiring needed in the scene.
- `SceneLoader.Initialize(inputManager, uiSystem)` is called by `GameManager` - no scene-level ref needed.
- `DontDestroyOnLoad` is applied by `GameManager.Awake()` on the GameManager GO.

### Manual steps still to do
- None for this scene wiring.
- Boot.unity must be in Build Settings index 0; Persistent at index 1 (handled in "register scenes" task).
- Full end-to-end boot test after Menu and Game scenes are wired.

## wire menu scene

- [x] AI: wire `Menu.unity` (Play / Continue / Quit buttons routed through GameManager) -- done via MCP 2026-05-04

### What was implemented (MCP)

**New files created:**
- `Assets/UI/Menu.uxml` - menu layout with `btn-play`, `btn-continue`, `btn-quit` buttons
- `Assets/UI/Menu.uss` - styles matching project palette (1920x1080 landscape, touch targets >= 96 px)
- `Assets/Scripts/Core/MenuController.cs` - `Game.Core` MonoBehaviour; placed in Core (not UI) because it directly calls `GameManager.Instance` - circular dep with `Game.UI` asmdef is avoided this way

**MenuController behaviour:**
- `Start()` queries buttons from UIDocument root, wires `.clicked` callbacks
- `btn-play` -> `GameManager.Instance.StartNewGameAsync()`
- `btn-continue` -> `GameManager.Instance.ContinueAsync()`; disabled if `SaveSystem.HasSave == false`
- `btn-quit` -> `GameManager.Instance.QuitGame()`
- `OnDestroy()` unsubscribes all button handlers (no leaks)

**GameObject + wired refs:**

| GameObject | Component(s) | Serialized refs wired |
|---|---|---|
| `Menu` | `Game.Core.MenuController`, `UIDocument` | `_menuDocument` -> UIDocument; UIDocument `sourceAsset` -> Menu.uxml, `m_PanelSettings` -> GamePanelSettings |

Console: 0 errors, 0 warnings after save (pre-existing CS4014 in GameBootstrap is unrelated).

### Final Menu.unity hierarchy
```
Menu.unity
└── Menu  [Transform, MenuController, UIDocument]  <- Menu.uxml + GamePanelSettings
```

### Manual steps still to do
- None for this scene wiring.
- Build Settings must include all scenes (Boot first) before end-to-end test.

## wire game scene

- [x] AI: wire `Game.unity` minimum (sea plane, ship spawn, camera) -- done via MCP 2026-05-04

### What was implemented (MCP)

**No new scripts or assets** - Phase 6 scripts (ShipController, CameraController) do not exist yet; structural GOs only.

**GameObjects + components:**

| GameObject | Component(s) | Notes |
|---|---|---|
| `DirectionalLight` | `Transform`, `Light`, `UniversalAdditionalLightData` | rotation (50, -30, 0), type = Directional |
| `Sea` | `Transform`, `MeshFilter`, `MeshCollider`, `MeshRenderer` | Plane primitive, scale (20, 1, 20), pos (0, 0, 0) |
| `Ship` | `Transform`, `MeshFilter`, `BoxCollider`, `MeshRenderer` | Cube primitive, scale (1, 0.5, 2), pos (0, 0.5, 0) - placeholder for Phase 6 PF_Ship prefab |
| `MainCamera` | `Transform`, `Camera`, `UniversalAdditionalCameraData` | tag = "MainCamera", pos (0, 3, -6), rotation (15, 0, 0) - Phase 6 adds CameraController |

Console: pre-existing CS4014 warning only (no new errors or warnings).

### Final Game.unity hierarchy
```
Game.unity
├── DirectionalLight  [Transform, Light, UniversalAdditionalLightData]
├── Sea               [Transform, MeshFilter, MeshCollider, MeshRenderer]
├── Ship              [Transform, MeshFilter, BoxCollider, MeshRenderer]
└── MainCamera        [Transform, Camera, UniversalAdditionalCameraData]  tag=MainCamera
```

### Notes
- `Ship` is a cube placeholder. Phase 6 will replace it with the `PF_Ship` prefab and add `ShipController`.
- `MainCamera` is tagged "MainCamera" so `Camera.main` returns it at runtime. Phase 6 adds `CameraController`.
- Sea plane scale 20x20 units. Phase 6 can resize/replace with actual sea geometry.

### Manual steps still to do
- None for this scene wiring.
- Build Settings must include all scenes (Boot first) before end-to-end test.

## register scenes

- [x] AI: register all scenes in Build Settings (Boot first) -- done via MCP 2026-05-04

### What was implemented

Used `manage_build scenes` to set `EditorBuildSettings.scenes`:

| Index | Scene | Enabled |
|-------|-------|---------|
| 0 | `Assets/Scenes/Boot.unity` | true |
| 1 | `Assets/Scenes/Persistent.unity` | true |
| 2 | `Assets/Scenes/Menu.unity` | true |
| 3 | `Assets/Scenes/Game.unity` | true |

Console: pre-existing CS4014 warning only (no new errors).

## test in editor

- [ ] HUMAN: press Play in `Boot.unity` - expect Menu reachable, no console errors

### Test run 1 - results

**What worked:**
- Boot scene started, all systems initialized (logs visible in Console)
- Menu appeared: New Game clickable, Continue disabled (no save), Quit present
- Clicking New Game transitioned to Game scene, ship visible at center

**Issues found:**

#### Issue 1 -- "display 1 no cameras rendering" on Menu

**Root cause:** Menu.unity had no camera. When Boot unloads (additive load of Persistent, then unload of Boot), the only active scene is Persistent + Menu - neither had a camera, so Unity showed the "no cameras rendering" overlay.

**Fix applied (MCP 2026-05-04):**
- Added `MenuCamera` GameObject to Menu.unity: `[Transform, Camera, UniversalAdditionalCameraData, AudioListener]`
- Tagged `MainCamera`, position (0, 0, -10), default rotation
- Saved Menu.unity

**Also found:** `no audio listener` warning from two boot states (menu + game). Each scene camera should own its AudioListener since scenes load/unload exclusively (Menu unloads before Game loads, so there is never a duplicate listener).

**Fix applied (MCP 2026-05-04):**
- Added `AudioListener` to `MenuCamera` in Menu.unity
- Added `AudioListener` to `MainCamera` in Game.unity
- Both scenes saved

**Updated Menu.unity hierarchy:**
```
Menu.unity
├── Menu        [Transform, MenuController, UIDocument]  <- Menu.uxml + GamePanelSettings
└── MenuCamera  [Transform, Camera, UniversalAdditionalCameraData, AudioListener]  tag=MainCamera  pos(0,0,-10)
```

**Updated Game.unity hierarchy:**
```
Game.unity
├── DirectionalLight  [Transform, Light, UniversalAdditionalLightData]
├── Sea               [Transform, MeshFilter, MeshCollider, MeshRenderer]
├── Ship              [Transform, MeshFilter, BoxCollider, MeshRenderer]
└── MainCamera        [Transform, Camera, UniversalAdditionalCameraData, AudioListener]  tag=MainCamera  pos(0,3,-6)
```

#### Issue 2 -- No pause / virtual stick buttons in Game scene

**Root cause:** Expected. Pause button, virtual stick, and in-game HUD are Phase 6 work (see `tracking.md` Phase 6). The `UIRoot/Pause` document exists in Persistent but UISystem only shows it when `GameManager` transitions to `Paused` state, which requires Phase 6 ShipController input wiring.

**Action:** None for Phase 5. Confirmed as Phase 6 scope.

#### Issue 3 -- "Quality level 'Mobile' not found" warning

**Root cause:** `GameBootstrap` calls `QualitySettings.SetQualityLevel` by name for a level called "Mobile" which has not been added to this project's quality settings. The project uses Unity's default quality tiers.

**Debug steps to verify:**
1. Open Edit -> Project Settings -> Quality
2. Check the list of quality level names
3. Either add a "Mobile" level or update `GameBootstrap` to use an existing level name

**Action:** Not blocking for Phase 5 but should be resolved before Phase 6 testing.

### Test run 2 - actions to take after fixes

After pressing Play in Boot.unity again, record:

| Check | Expected | Actual |
|-------|----------|--------|
| Console errors | 0 errors | |
| Console warnings | CS4014 only (GameBootstrap line 139) | |
| "no cameras rendering" overlay | Gone | |
| "no audio listener" warnings | Gone | |
| "Quality level Mobile not found" | Still present (not yet fixed) | |
| Menu loads | New Game enabled, Continue disabled | |
| Click New Game | Game scene loads, ship visible | |
| Click Quit from menu | Application quits (no-op in Editor) | |
