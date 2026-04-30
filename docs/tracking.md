# Setup & Build Roadmap

A linear checklist from empty repo to private alpha. **Order matters.** Each
item links to the doc that defines it. Items prefixed `HUMAN:` need a person;
`AI:` can be delegated to Copilot/agent following the rules in
[ai-development-playbook.md](ai-development-playbook.md).

Mark complete with `[x]`. Add a date + commit/tag in parentheses if useful.

---

## Phase 0 - Account & accounts (do early, gates everything else)

- [ ] HUMAN: Create / log in to Google Play Console; pay $25 fee - see [google-play-private-alpha.md](google-play-private-alpha.md#2-prerequisites)
- [ ] HUMAN: Complete Play Console identity verification (can take days)
- [ ] HUMAN: Reserve app package name (e.g. `com.<org>.artificialpi`)
- [ ] HUMAN: Host a privacy policy URL (GitHub Pages is fine)

---

## Phase 1 - Local toolchain

- [x] HUMAN: Install Unity Hub
- [x] HUMAN: Install Unity Editor **6000.xxx LTS** with Android Build Support module - see [dev-environment-setup.md §2](dev-environment-setup.md#2-required-software-versions-are-pinned)
- [x] HUMAN: Install VS Code + extensions (C# Dev Kit, Unity, Copilot, EditorConfig)
- [x] HUMAN: Install .NET SDK 8.0+
- [x] HUMAN: Install Git ≥ 2.40 and Git LFS ≥ 3.4; run `git lfs install` once
- [x] HUMAN: Install `bundletool` (for AAB smoke-testing)
- [x] HUMAN: Verify `adb devices` works with a real Android device (Developer Options + USB debugging on)

---

## Phase 2 - Repository scaffolding (no Unity project yet)

- [x] AI: create `.gitignore` per [git-workflow.md §3](git-workflow.md#3-gitignore-essentials-unity)
- [x] AI: create `.gitattributes` per [git-workflow.md §4](git-workflow.md#4-gitattributes-lfs--required)
- [x] AI: create `.editorconfig` matching [coding-standards.md](coding-standards.md)
- [x] AI: create `.github/copilot-instructions.md` from [copilot-instructions-template.md](copilot-instructions-template.md)
- [x] AI: create `AGENTS.md` (short, links to `docs/`)
- [x] AI: create `Tools/git-hooks/pre-commit` per [git-workflow.md §6](git-workflow.md#6-pre-commit-guardrails)
- [x] AI: create `Tools/setup.sh` (installs LFS hook, copies `build.env.example`, verifies tooling)
- [x] HUMAN: run `Tools/setup.sh` once; confirm pre-commit hook is active

---

## Phase 3 - Unity project creation

- [x] HUMAN: in Unity Hub, create new **3D (URP) Mobile** project at repo root, Unity 6 LTS
- [x] HUMAN: commit initial Unity project (`Assets/`, `Packages/`, `ProjectSettings/`, `UserSettings/` excluded by gitignore)
- [x] HUMAN: verify `git lfs ls-files` shows binary assets after first commit
- [x] AI: create folder skeleton per [project-structure.md §2](project-structure.md#2-top-level-layout) (empty folders + `.gitkeep`)
- [x] AI: create asmdefs per [project-structure.md §3](project-structure.md#3-assembly-definitions-asmdef)
- [x] AI: configure Player Settings per [build-and-release.md §3](build-and-release.md#3-android-player-settings-locked) (commit `ProjectSettings/`)
- [x] AI: create URP asset (mobile profile) in `Assets/Settings/`; assign in Graphics + Quality
- [x] AI: create `Assets/Settings/InputActions.inputactions` per [systems/input-manager.md](systems/input-manager.md)
- [x] HUMAN: confirm Editor opens with **0 errors / 0 warnings** after this phase

---

## Phase 4 - Core systems (one PR per system, in this order)

- [x] AI: implement [systems/logging.md](systems/logging.md) (no dependencies)
- [x] AI: implement [systems/save-system.md](systems/save-system.md) (+ EditMode tests)
- [ ] AI: implement [systems/input-manager.md](systems/input-manager.md)
- [ ] AI: implement [systems/audio-manager.md](systems/audio-manager.md)
- [ ] AI: implement [systems/ui-system.md](systems/ui-system.md) (loading + pause overlays)
- [ ] AI: implement [systems/scene-loader.md](systems/scene-loader.md)
- [ ] AI: implement [systems/game-manager.md](systems/game-manager.md) (+ state-machine EditMode tests)
- [ ] AI: implement [systems/game-bootstrap.md](systems/game-bootstrap.md)

---

## Phase 5 - Scenes

- [ ] HUMAN: create `Boot.unity`, `Persistent.unity`, `Menu.unity`, `Game.unity` (commit empty)
- [ ] AI: wire `Boot.unity` (single `GameBootstrap`)
- [ ] AI: wire `Persistent.unity` (GameManager, all systems, UIRoot)
- [ ] AI: wire `Menu.unity` (Play / Continue / Quit buttons routed through GameManager)
- [ ] AI: wire `Game.unity` minimum (sea plane, ship spawn, camera)
- [ ] AI: register all scenes in Build Settings (Boot first)
- [ ] HUMAN: press Play in `Boot.unity` - expect Menu reachable, no console errors

---

## Phase 6 - Gameplay vertical slice

- [ ] AI: implement [systems/ship-controller.md](systems/ship-controller.md) (+ tests for input clamping)
- [ ] AI: implement [systems/camera-controller.md](systems/camera-controller.md)
- [ ] AI: create stylized water shader (URP Shader Graph) - minimal animation
- [ ] AI: create island prefab (static mesh, no collisions in v0.2)
- [ ] AI: implement on-screen virtual stick + pause button (UI Toolkit overlay)
- [ ] AI: wire SaveSystem into GameManager pause/quit and Menu Continue button
- [ ] HUMAN: play in Editor - Menu → Game → drive → Pause → Resume → Menu → Continue restores position

---

## Phase 7 - Tests & quality gates

- [ ] AI: PlayMode test: full Boot → Menu → Game → Menu loop, asserts no leaked GameObjects
- [ ] AI: PlayMode test: pause → resume preserves ship transform
- [ ] AI: PlayMode test: save → reload scene → Continue restores position
- [ ] HUMAN: run all tests in Test Runner - all green

---

## Phase 8 - Build pipeline

- [ ] AI: implement `Game.Editor.BuildPipeline` per [build-and-release.md §6](build-and-release.md#6-build-script-toolsbuild-androidsh)
- [ ] AI: implement `Tools/build-android.sh` (`dev` / `profile` / `release` modes)
- [ ] AI: implement `build.env.example`
- [ ] HUMAN: generate debug keystore (Unity does this on first build) - verify gitignored
- [ ] HUMAN: `./Tools/build-android.sh dev` produces installable APK
- [ ] HUMAN: install on device, run, watch `adb logcat -s Unity:V` - no errors

---

## Phase 9 - MCP integration (optional but recommended)

- [ ] HUMAN: choose MCP server (justinpbarnett/unity-mcp or alternative) - see [mcp-setup.md §2](mcp-setup.md#2-server-choice). Record choice + pinned version here: `___________`
- [ ] HUMAN: add MCP package to `Packages/manifest.json` pinned
- [ ] AI: create `.vscode/mcp.json` per [mcp-setup.md §4](mcp-setup.md#4-vs-code--copilot-client-config)
- [ ] HUMAN: in Copilot Chat, confirm `unity` MCP server connects
- [ ] HUMAN: smoke prompt - ask agent to list root GameObjects in Boot scene; verify

---

## Phase 10 - Performance pass

- [ ] HUMAN: build `profile` APK, attach Unity Profiler to device
- [ ] HUMAN: capture 60 s gameplay; compare against [performance-budget.md](performance-budget.md)
- [ ] HUMAN: capture Frame Debugger snapshot; verify draw calls ≤ 80
- [ ] AI: address regressions (in priority order from [performance-budget.md §7](performance-budget.md#7-optimization-order-when-over-budget))
- [ ] HUMAN: re-profile until budget met for 5 min sustained play

---

## Phase 11 - Generate upload key

- [ ] HUMAN: generate upload keystore (`keytool -genkeypair -keystore upload.keystore -alias upload -keyalg RSA -keysize 2048 -validity 10000`)
- [ ] HUMAN: store keystore + passwords in password manager. **Do not commit.**
- [ ] HUMAN: fill in `~/.config/artificial-pi/build.env` with real values
- [ ] HUMAN: `./Tools/build-android.sh release` produces signed AAB

---

## Phase 12 - Play Console setup

- [ ] HUMAN: create app in Play Console - see [google-play-private-alpha.md §4](google-play-private-alpha.md#4-app-creation-one-time)
- [ ] HUMAN: complete every pre-launch form in [§5](google-play-private-alpha.md#5-required-pre-launch-forms-even-for-internal-track)
- [ ] HUMAN: opt in to Play App Signing on first AAB upload
- [ ] HUMAN: create Internal Testing track; add tester emails / Google Group
- [ ] HUMAN: upload first release AAB; bump versionCode if needed
- [ ] HUMAN: share opt-in URL with at least one external tester
- [ ] HUMAN: confirm tester can install via Play Store and launch the app
- [ ] HUMAN: verify the [release checklist](build-and-release.md#9-release-checklist) is fully checked

---

## Phase 13 - Definition of Demo-Ready

- [ ] HUMAN: run every item in [functional-specs.md §12](functional-specs.md#12-definition-of-demo-ready) on the Play Store-installed build
- [ ] HUMAN: tag the commit `v0.2.0` once all green

---

## Notes / parking lot

Free-form. Move items into the phases above when they become actionable.

- _(empty)_
