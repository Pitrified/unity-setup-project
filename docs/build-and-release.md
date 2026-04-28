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
