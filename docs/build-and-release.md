# Build & Release Process

## 1. Goal

A **single command** produces a signed AAB that is uploadable to Play Console.

```
./Tools/build-android.sh release
```

No clicking through Editor menus for releases.

---

## 2. Build matrix

| Build type | Output                  | Signing       | Stripping    | When                  |
| ---------- | ----------------------- | ------------- | ------------ | --------------------- |
| `dev`      | APK, attached to device | Debug keystore | Off          | Local iteration       |
| `profile`  | APK, IL2CPP             | Debug keystore | Off          | Profiling on device   |
| `release`  | AAB                     | Upload key    | Strip Engine Code, Managed: Low | Play upload    |

---

## 3. Android player settings (locked)

| Setting                        | Value                                  |
| ------------------------------ | -------------------------------------- |
| Scripting backend              | IL2CPP                                 |
| Target architectures           | ARM64 only                             |
| API level - minimum            | 26 (Android 8.0)                       |
| API level - target             | 35 (Android 15) - Play requirement     |
| Orientation                    | Landscape Left + Landscape Right       |
| Internet access                | Auto                                   |
| Write permission               | Internal                               |
| Color space                    | Linear                                 |
| Graphics APIs                  | Vulkan, then GLES3                     |
| Multithreaded rendering        | On                                     |
| Optimized frame pacing         | On                                     |
| Compression method             | LZ4HC (release), LZ4 (dev)             |

These live in `ProjectSettings/ProjectSettings.asset` - committed.

---

## 4. Versioning

Single source of truth: `ProjectSettings/ProjectSettings.asset`.

| Field         | Format        | Bumped by                    |
| ------------- | ------------- | ---------------------------- |
| `bundleVersion` (versionName) | `MAJOR.MINOR.PATCH-channelN` (e.g. `0.2.0-alpha3`) | Human, per release |
| `AndroidBundleVersionCode`   | Monotonic integer | Build script, auto-increment on `release` |

Build script writes the new version code back to disk and stages it.

Tag format: `v0.2.0-alpha3` on the commit that produced the AAB.

---

## 5. Signing

- **Debug keystore:** Unity-managed, ignored by git, fine to regenerate.
- **Upload keystore:** generated once, stored in a password manager, **never committed**.
  - Algorithm: RSA 2048, validity ≥ 25 years.
  - Filename convention: `keystore/upload.keystore` (folder gitignored).
- **App signing key:** managed by Google Play (Play App Signing). Do not export.

Local environment vars (e.g. `~/.config/artificial-pi/build.env`, gitignored):

```
ANDROID_KEYSTORE_PATH=/abs/path/upload.keystore
ANDROID_KEYSTORE_PASS=...
ANDROID_KEY_ALIAS=upload
ANDROID_KEY_PASS=...
```

The build script reads these; missing values → fail loudly, never silently
fall back to debug signing.

---

## 6. Build script (Tools/build-android.sh)

Spec (no implementation here):

1. Validate env vars for the requested mode.
2. Resolve Unity binary from `UNITY_PATH` env var or Unity Hub default.
3. Invoke Unity in batchmode:
   ```
   unity -batchmode -nographics -quit \
         -projectPath . \
         -buildTarget Android \
         -executeMethod Game.Editor.BuildPipeline.Build<Type> \
         -logFile build.log
   ```
4. On success, print the absolute path of the artifact and its SHA256.
5. Auto-increment `AndroidBundleVersionCode` for `release`.
6. Exit non-zero on any Unity error in `build.log`.

The Unity-side `Game.Editor.BuildPipeline` lives in `Assets/Scripts/Editor/`
and is committed.

---

## 7. Manual release flow

1. `git status` clean, on `main`, up to date.
2. Bump `bundleVersion` in ProjectSettings (human edit).
3. `./Tools/build-android.sh release`
4. Smoke-test the AAB with `bundletool` on a real device:
   ```
   bundletool build-apks --bundle=Build/release.aab --output=run.apks --connected-device
   bundletool install-apks --apks=run.apks
   ```
5. Run the [§9 release checklist](#9-release-checklist).
6. Tag commit, push tag.
7. Upload AAB to Play Console - see [google-play-private-alpha.md](google-play-private-alpha.md).

---

## 8. Artifact retention

- `Build/` is gitignored.
- Keep the last 3 release AABs locally.
- Upload key-signed AABs are also archived in Play Console automatically.

---

## 9. Release checklist

- [ ] Version code strictly greater than last upload
- [ ] Version name updated and matches git tag
- [ ] Build is `release` (IL2CPP, ARM64, stripped, signed with upload key)
- [ ] App launches on physical device, runs the demo loop
- [ ] No errors in `adb logcat -s Unity` during 5 minutes of play
- [ ] Permissions in manifest match what app actually uses
- [ ] AAB size noted (target < 150 MB, hard cap 200 MB without asset packs)

---

## 10. Future improvements

- GitHub Actions: build AAB on tag push, upload to Internal track via Play Developer API.
- Automated symbol upload (IL2CPP `.symbols.zip`) for crash readability.
- Pre-merge: build dev APK on PR, fail fast.

---

## 11. First dev build walkthrough (step-by-step)

This section walks through Phase 8's HUMAN steps for someone new to Unity command-line
builds. Follow each step in order; check the verification step before moving on.

---

### 11.1 Prerequisites

Before running the build script, confirm these are in place:

| Requirement | How to check |
| --- | --- |
| Unity 6000.x installed via Unity Hub | `ls ~/Unity/Hub/Editor/6000.*/Editor/Unity` - should print a path |
| Android Build Support module installed | Open Unity Hub → Installs → click the gear on your Unity version → Add modules → Android Build Support |
| Android SDK present | The Android Build Support module includes the SDK automatically |
| Physical Android device connected | Enable Developer Options on the device: Settings → About → tap Build Number 7 times; then Settings → Developer Options → USB Debugging ON |
| ADB works | Run `adb devices`; device must appear as `device` (not `unauthorized`) |
| Pre-commit hook installed | `ls .git/hooks/pre-commit` - should exist and link to `Tools/git-hooks/pre-commit` |

If ADB is not on your PATH, see the note printed by `Tools/setup.sh` about the Unity SDK path.

---

### 11.2 Run setup (once per machine)

```bash
# From the repo root:
./Tools/setup.sh
```

This creates `~/.config/artificial-pi/build.env` from `build.env.example` if it does not
already exist. The file only needs to be filled in for release builds (keystore settings).
For dev builds, leaving it as-is is fine.

**Verification:** `ls ~/.config/artificial-pi/build.env` should exist.

---

### 11.3 Run the dev build

```bash
# From the repo root:
./Tools/build-android.sh dev
```

What happens:
1. The script resolves the Unity binary from `~/Unity/Hub/Editor/...`.
2. Unity opens in headless mode (no window).
3. `Game.Editor.BuildPipeline.BuildDev` is called.
4. Unity compiles IL2CPP and links the APK (~5-15 minutes for the first build).
5. The APK is written to `UnitySetupPrpj/Build/dev/artificial-pi-dev.apk`.
6. The script prints the artifact path, file size, and SHA256.

**What success looks like:**
```
[build]  OK    Unity binary: ~/Unity/Hub/Editor/6000.4.4f1/Editor/Unity
[build]        Starting Unity build...
[build]  OK    Build SUCCEEDED
[build]        Artifact : .../UnitySetupPrpj/Build/dev/artificial-pi-dev.apk
[build]        Size     : 87M
[build]        SHA256   : a1b2c3...
```

**If it fails:** the script prints the last 40 lines of the build log. The full log is at
`UnitySetupPrpj/Build/build-dev.log`. Search it for `error CS` (compile errors) or
`Error` (runtime Unity errors).

---

### 11.4 Verify the debug keystore is gitignored

On the first Android build, Unity auto-generates a debug keystore. Verify it will not be
accidentally committed:

```bash
# Should show nothing (no untracked keystore files):
git status --short | grep -i keystore || echo "OK - no keystore files visible to git"

# Confirm .gitignore covers the pattern:
git check-ignore -v UnitySetupPrpj/keystore/ 2>/dev/null \
  || echo "keystore/ is gitignored (or does not exist yet)"
```

The repo's `.gitignore` already includes `keystore/` and `*.keystore`, so Unity's
auto-generated debug keystore is covered. You never need to add it manually.

---

### 11.5 Install on device

```bash
# Confirm the device is recognized:
adb devices
# Expected output example:
#   List of devices attached
#   R58T309JZXY    device      <- "device" = ready; "unauthorized" = enable USB debugging

# Install the APK (replace existing install if present):
adb install -r UnitySetupPrpj/Build/dev/artificial-pi-dev.apk
```

**Expected output:** `Success` at the end of the adb install output.

If you see `INSTALL_FAILED_CPU_ABI_INCOMPATIBLE`: the device is 32-bit only;
the build targets ARM64. This is an unsupported device for this project.

---

### 11.6 Launch and monitor logs

Open two terminal windows:

**Terminal 1 - Start log capture before launching the app:**
```bash
adb logcat -c                    # clear existing logs
adb logcat -s Unity:V            # show only Unity log lines, verbose level
```

**Terminal 2 - Launch the app:**
```bash
adb shell am start \
    -n com.DefaultCompany.UnitySetupPrpj/com.unity3d.player.UnityPlayerGameActivity
```

Replace the package/activity name with your actual package name from Player Settings.

**Expected log output (good):**
```
V Unity   : [INFO][Boot] Runtime defaults applied: fps=60, orientation=...
V Unity   : [INFO][Boot] GameState: Boot -> Loading
V Unity   : [INFO][Boot] Persistent scene loaded.
V Unity   : [INFO][Boot] GameManager initialized.
V Unity   : [INFO][Boot] Handoff complete.
V Unity   : [INFO][Boot] GameState: Loading -> Menu
```

**Red flags (investigate if seen):**
```
E Unity   : ...Exception...          <- any exception is a bug
E Unity   : NullReferenceException   <- missing component reference
E Unity   : GameManager not found    <- Persistent scene wiring broken
```

**Test the demo loop manually:**
1. Tap Play on the Menu screen.
2. Drive the ship using the on-screen stick.
3. Tap the pause button; confirm the pause overlay appears.
4. Tap Resume; ship is in the same spot.
5. Tap Return to Menu; Menu appears.
6. Tap Continue; ship is back where you left it.

No errors in `adb logcat` throughout = Phase 8 complete.

