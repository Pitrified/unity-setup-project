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
| Capture | `-executeMethod <Class.Method>` of an editor script that renders a camera to a PNG | the PNG looks right, and measures right where the change is visual |

`-runTests` must not be combined with `-quit`. A failing test's message is in the XML's `failure/message`.

`build-android.sh` finds the editor under `~/Unity/Hub/Editor/` by itself; `UNITY_PATH` overrides it.
To read the APK's id, label and ABI: `aapt2 dump badging <apk>`, with `aapt2` from the editor's `AndroidPlayer/SDK/build-tools/`.

---

## 4. Changing assets without the Editor

Scenes, prefabs, ScriptableObjects and project settings are YAML that Unity writes; edit them through Unity, not by hand.

- Write an editor method (`EditorSceneManager`, `AssetDatabase`, `SerializedObject`, `PlayerSettings`) and run it with `-executeMethod`.
- A one-off migration lives in an `Editor` folder while it runs and is deleted before the commit; the plan log records what it did.
- Read the YAML diff before committing: a change should touch what it meant to and nothing else.
- After deleting an asset, check that nothing references its GUID: `git grep <guid>` over the project, excluding `.meta` files.

---

## 5. Pitfalls seen here

- **First import of a fresh `Library/`** fails once inside the Shader Graph package (`error CS0246: 'GUID' could not be found`); Unity's API updater fixes it and the next run compiles. Expect it once per new clone.
- **Do not copy `Library/` between clones.** A copied one compiled, then every scene component came up as "referenced script is missing" in PlayMode. Delete it and let Unity import (a few minutes).
- **An interrupted build** leaves `Assets/Resources/PerformanceTestRun*.json` behind (the performance test package's build step). Delete it; a finished build removes it itself.
- **PlayMode tests that boot the game** must destroy the `DontDestroyOnLoad` `GameManager` in teardown (`PlayModeTestReset`), or the next test waits on the old instance.
- **Animated shaders** (the water uses `_Time`) make two captures differ even with no change. Compare a change against a control pair of captures, not against zero.
