# Scratch Pad - artificial-pi - Phase 8 - Build Pipeline

Implementing Phase 8 from [tracking.md](../docs/tracking.md)

Reference docs: [build-and-release.md](../docs/build-and-release.md)

---

## Tasks

- [x] AI: implement `Game.Editor.BuildPipeline` per build-and-release.md §6
- [x] AI: implement `Tools/build-android.sh` (`dev` / `profile` / `release` modes)
- [x] AI: implement `build.env.example`
- [ ] HUMAN: generate debug keystore (Unity does this on first build) - verify gitignored
- [ ] HUMAN: `./Tools/build-android.sh dev` produces installable APK
- [ ] HUMAN: install on device, run, watch `adb logcat -s Unity:V` - no errors

---

## Relevant docs by task

| Task | Doc reference |
| --- | ----- |
| BuildPipeline.cs | build-and-release.md §2 (matrix), §3 (player settings), §6 (spec) |
| build-android.sh | build-and-release.md §6 (shell spec), §4 (versioning), §5 (signing) |
| build.env.example | build-and-release.md §5, setup.sh (copies it to ~/.config/artificial-pi/build.env) |

---

## Key Decisions

### BuildPipeline.cs

- Static entry points: `BuildDev`, `BuildProfile`, `BuildRelease` - all called via
  `-executeMethod Game.Editor.BuildPipeline.Build*`.
- Build modes:
  - `dev`: IL2CPP, ARM64, APK, `Development` flag, no stripping, debug keystore (project default)
  - `profile`: IL2CPP, ARM64, APK, `Development | EnableDeepProfilingSupport`, no stripping
  - `release`: IL2CPP, ARM64, AAB, upload keystore from env vars, strip engine + managed Low
- `BuildRelease` caches original `PlayerSettings.Android.keystoreName|Pass|keyaliasName|Pass`
  in a `try/finally` so they are restored after a successful build.
  This keeps `ProjectSettings.asset` clean (only the version code bump is left).
  On failure (exception thrown by `RunBuild`), Unity exits with code 1 before writing settings.
- `RunBuild` throws `BuildFailedException` on failure rather than calling
  `EditorApplication.Exit(1)`, so `finally` blocks run on the happy path.
- `GetRequiredEnv` throws `InvalidOperationException` for missing env vars - propagates
  to Unity's executeMethod handler → exit code 1 in shell.
- Auto-increment `AndroidBundleVersionCode` (release only) happens in C# before the build.
  Shell script stages `ProjectSettings/ProjectSettings.asset` after success.
- `LINQ` used in `GetEnabledScenePaths` (cold path; not a hot path - allowed by standards).

### build-android.sh

- Resolves Unity binary from `UNITY_PATH` env var first, then scans Unity Hub defaults
  (Linux `~/Unity/Hub/Editor/*/Editor/Unity`, macOS glob for `.app/Contents/MacOS/Unity`).
- Sources `~/.config/artificial-pi/build.env` if it exists (set `UNITY_PATH` there too).
- Release mode validates all four keystore env vars before calling Unity.
- Parses Unity exit code + scans `build.log` for error markers.
- After successful release build, stages `ProjectSettings/ProjectSettings.asset`.
- Prints artifact path + size + SHA256 on success.

### build.env.example

- Lives at repo root (setup.sh references `$REPO_ROOT/build.env.example`).
- `build.env` is gitignored; `build.env.example` is committed.

### Known limitations

- On release build FAILURE: Unity may write the temp keystore path to
  `ProjectSettings.asset` before exiting. Run
  `git checkout -- UnitySetupPrpj/ProjectSettings/ProjectSettings.asset` if this happens.
- `keystore/` and `*.keystore` are already gitignored.
- Shell script does NOT auto-stage `ProjectSettings.asset` on `dev` or `profile` builds
  (version code is only bumped for `release`).

---

## What Was Done

### Game.Editor.BuildPipeline.cs
- Created `Assets/Scripts/Editor/BuildPipeline.cs`
- Three public static entry points: `BuildDev`, `BuildProfile`, `BuildRelease`
- `try/finally` in `BuildRelease` restores original signing settings
- `RunBuild` throws `BuildFailedException` on failure
- Verified: 0 compile errors after MCP refresh

### Tools/build-android.sh
- Created `Tools/build-android.sh` (chmod +x via shell)
- `dev` / `profile` / `release` modes
- Auto-stages ProjectSettings on release success

### build.env.example
- Created `build.env.example` at repo root

---

## Manual Steps (HUMAN)

1. Open Unity Editor with `Boot.unity` active.
2. Try `./Tools/build-android.sh dev` - Unity Hub must be installed, Android Build Support module present.
3. On first build, Unity auto-generates a debug keystore in `UnitySetupPrpj/keystore/` - verify it is gitignored.
4. Locate the APK in `UnitySetupPrpj/Build/dev/artificial-pi-dev.apk`.
5. `adb install UnitySetupPrpj/Build/dev/artificial-pi-dev.apk`
6. Launch on device, watch `adb logcat -s Unity:V` - no errors.
