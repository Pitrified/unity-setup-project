# Running Unity headless

How a session with no screen (a Claude Code session on a Linux box over SSH) opens the project, runs the tests, builds the APK and renders images.
These are the gates every change closes on; [ai-development-playbook.md](ai-development-playbook.md) says when each one applies.
A person at the Editor uses the Test Runner and Play mode instead; the results must agree.

Verified on Ubuntu 26.04 with an NVIDIA GPU, Unity `6000.4.4f1`, 2026-11-17.

---

## 1. Install, once per machine

- **Unity CLI** (`unity`), installed per user to `~/.local/bin`. Read `https://unity.com/install.sh` before running it, and run it with `UNITY_CLI_SKIP_SETUP=1`: otherwise it ends by installing Unity's agent skills into every AI client it finds.
  Then `unity analytics opt-out`.
- **Editor and Android module**, at the version in `UnitySetup/ProjectSettings/ProjectVersion.txt`:
  ```
  unity install 6000.4.4f1 --changeset 360f97ecca93 -m android --cm --accept-eula --non-interactive
  ```
  It lands in `~/Unity/Hub/Editor/6000.4.4f1/`, about 18 GB with the SDK, NDK and OpenJDK.
- **Xvfb** (`apt install xvfb`, needs sudo). The editor does not start without an X display, even in batch mode.
- **Licence**, Unity Personal, from the terminal with no GUI:
  ```
  unity auth login
  unity license activate --personal --accept-eula
  unity license status
  ```
  Batch runs then log `[Licensing::Module] Error: Access token is unavailable` and work regardless.

---

## 2. The command

Every run below has the same shape:

```
EDITOR=~/Unity/Hub/Editor/6000.4.4f1/Editor/Unity
xvfb-run -a -s "-screen 0 1280x1024x24" "$EDITOR" -batchmode -force-vulkan \
  -projectPath UnitySetup -logFile <log> <what to run>
```

- `xvfb-run` gives the editor a virtual display.
- `-force-vulkan` makes it render on the GPU. Without it, under Xvfb, it falls back to Mesa `llvmpipe`, which is the CPU.
- Do not pass `-nographics` to anything that renders (PlayMode tests, captures): frames come back black.
- The exit code is the result. The log holds the detail: `grep -E "error CS|warning CS" <log>`.
- One Unity process per project at a time; a second one fails on the project lock.

---

## 3. Gates

| Gate | `<what to run>` | Pass |
| ---- | --------------- | ---- |
| Compile | `-quit` | exit 0, no `error CS` and no `warning CS` lines in the log |
| EditMode tests | `-runTests -testPlatform EditMode -testResults <xml>` | exit 0; the XML root has `failed="0"` |
| PlayMode tests | `-runTests -testPlatform PlayMode -testResults <xml>` | exit 0; the XML root has `failed="0"` |
| `dev` APK | `Tools/build-android.sh dev`, itself under `xvfb-run -a` | `Build SUCCEEDED`; the APK under `UnitySetup/Build/dev/` |
| Capture | `-executeMethod Game.Editor.SceneCapture.CaptureFromCommandLine -captureOut shot.png [-captureEye x,y,z -captureTarget x,y,z]` | the PNG looks right, and measures right where the change is visual |
| Sequence | `-runTests -testPlatform PlayMode -testFilter Game.Tests.PlayMode.SequenceCaptureTests` with `CAPTURE_SEQUENCE_DIR=<dir>` (and `CAPTURE_SECONDS`, `CAPTURE_FPS`, `CAPTURE_EYE`, `CAPTURE_TARGET`) | frames over time in Play mode, read with `measure.py sheet / gif / motion / track` |

`Tools/gates.sh [compile|edit|play|all]` runs the first three gates this way and exits non-zero on any `error CS`, `warning CS` or failed test, naming the failed tests; logs and results go to `UnitySetup/Logs/gates/`.
It uses the editor of the project's version and, on Linux, always `xvfb-run` when installed.

**A still is not enough for anything that moves** (water, animation, physics, particles): the sequence capture plays the Game scene in fixed 1/30 s steps, real physics included, and writes `frame_NNN.png` from a pose, so the same frames come out however slow the machine is.
In `Tools/asset_builders`: `measure.py sheet DIR out.png` (all frames on one image, to look at), `gif DIR out.gif` (to watch), `motion DIR` (change between frames, overall and per grid cell: is it moving, and where; a still scene measures about 0.02, the control), `track DIR X0 Y0 X1 Y1` (a box's brightness and bright centroid over time: something bobbing, a wave passing).
Without `CAPTURE_SEQUENCE_DIR` the same test only checks that the scene plays through, so the gates run it at no cost.

`-runTests` must not be combined with `-quit`. A failing test's message is in the XML's `failure/message`.

`build-android.sh` takes the project's editor version under `~/Unity/Hub/Editor/`, or the newest one there; `UNITY_PATH` overrides it.
To read the APK's id, label and ABI: `aapt2 dump badging <apk>`, with `aapt2` from the editor's `AndroidPlayer/SDK/build-tools/`.

---

## 4. Changing assets without the Editor

Scenes, prefabs, ScriptableObjects and project settings are YAML that Unity writes; edit them through Unity, not by hand.

- Write an editor method (`EditorSceneManager`, `AssetDatabase`, `SerializedObject`, `PlayerSettings`) and run it with `-executeMethod`.
- A one-off migration lives in an `Editor` folder while it runs and is deleted before the commit; the plan log records what it did.
  Name it for what it does (`PlaceLandmarks`), never for a plan or phase: source files do not mention plans, temporary ones included.
- Read the YAML diff before committing: a change should touch what it meant to and nothing else.
- An asset created and then followed by a reimport in the same method can leave the method holding a replaced object, which saves as `{fileID: 0}`. Load it again from its path just before assigning it.
- After deleting an asset, check that nothing references its GUID: `git grep <guid>` over the project, excluding `.meta` files.
- Rename a script or asset with its `.meta` (`git mv` both), so the GUID and every reference to it survive.
- A renamed serialized field loses its value at the next save of each scene or prefab that set it. Add `[FormerlySerializedAs("oldName")]` before that save, or set the field again through `SerializedObject`.
- A renamed class keeps its old name in each component's `m_EditorClassIdentifier`; `ForceReserializeAssets` does not refresh it. Set it through `SerializedObject.FindProperty("m_EditorClassIdentifier")`.

---

## 5. Pitfalls seen here

- **First import of a fresh `Library/`** fails once inside the Shader Graph package (`error CS0246: 'GUID' could not be found`); Unity's API updater fixes it and the next run compiles. Expect it once per new clone.
- **Do not copy `Library/` between clones, and delete it after moving the project folder.** A copied or moved one compiled and passed EditMode, then scene components came up as "referenced script is missing" in PlayMode. Delete it and let Unity import (a few minutes).
- **An interrupted build** leaves `Assets/Resources/PerformanceTestRun*.json` behind (the performance test package's build step). Delete it; a finished build removes it itself.
- **PlayMode tests that boot the game** must destroy the `DontDestroyOnLoad` `GameManager` in teardown (`PlayModeTestReset`), or the next test waits on the old instance.
- **Animated shaders** (anything using `_Time`, such as a water shader) make two captures differ even with no change. Compare a change against a control pair of captures, not against zero.
- **`WaitForEndOfFrame` never fires in batch mode**: a PlayMode test that yields it fails. Yield `null` and render the camera into a texture yourself.
- **The first `Camera.Render()` after opening a scene** in an editor method comes out without its realtime light: surfaces show ambient only and look near black. Render twice and keep the second frame.
- **`DISPLAY` set but not usable**: a session over SSH can inherit `DISPLAY=:0` from a desktop it cannot open, and Unity then fails with "Error opening default X display". Run under `xvfb-run` regardless, as `gates.sh` does.
- **A RAM-backed `/tmp`**: Ubuntu 25.10 and later mount `/tmp` as a tmpfs with a per-user quota. A project clone there fills RAM, and Unity's package import fails with error -122 (`EDQUOT`). Keep projects and their `Library/` on disk.
