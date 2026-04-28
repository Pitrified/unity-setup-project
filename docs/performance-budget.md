# Performance Budget

Reference device: **Pixel 7** (mid-range Android, 2022). All numbers are
targets for v0.2 and tested on-device, not in editor.

---

## 1. Frame & timing

| Metric                          | Target            | Hard fail    |
| ------------------------------- | ----------------- | ------------ |
| Sustained FPS                   | 60                | < 30         |
| 1% low FPS over 5 min           | ≥ 30              | < 24         |
| Main-thread frame time          | ≤ 16 ms           | > 33 ms      |
| Render-thread frame time        | ≤ 16 ms           | > 33 ms      |
| GC alloc per frame in gameplay  | 0 B               | > 1 KB       |

`Application.targetFrameRate = 60`. `QualitySettings.vSyncCount = 0`. Unity
"Optimized Frame Pacing" enabled (Player Settings).

---

## 2. Rendering

| Metric                  | Target                | Notes                    |
| ----------------------- | --------------------- | ------------------------ |
| SetPass calls           | ≤ 30                  | URP batching + SRP batcher |
| Draw calls              | ≤ 80                  |                          |
| Tris on screen          | ≤ 150 k               |                          |
| Real-time lights        | 1 directional         | No additional realtime   |
| Real-time shadows       | Off in v0.2           | Baked or none            |
| Post-processing         | None / minimal        | Color grading at most    |

URP asset (mobile profile) lives in `Assets/Settings/`. Only one URP asset is shipped.

---

## 3. Memory

| Metric                  | Target              | Hard fail   |
| ----------------------- | ------------------- | ----------- |
| Total used (Profiler)   | ≤ 400 MB            | > 700 MB    |
| Texture memory          | ≤ 150 MB            |             |
| Mesh memory             | ≤ 50 MB             |             |
| Managed heap            | ≤ 64 MB, no growth  | growing     |

---

## 4. Disk / install size

| Metric            | Target     | Hard fail |
| ----------------- | ---------- | --------- |
| AAB size          | ≤ 80 MB    | > 150 MB  |
| Install on device | ≤ 200 MB   | > 350 MB  |

Cold start (icon tap → Menu interactive): ≤ 5 s on Pixel 7.

---

## 5. Asset import settings (defaults)

| Asset class            | Setting                          |
| ---------------------- | -------------------------------- |
| Textures (color)       | ASTC 6x6, sRGB on, max 1024      |
| Textures (UI)          | ASTC 6x6, sRGB on, max 1024, mipmaps off |
| Textures (normal)      | ASTC 6x6, sRGB off, normal map on |
| Audio (SFX, < 1 s)     | Decompress on Load, Vorbis q70   |
| Audio (music)          | Streaming, Vorbis q60            |
| Meshes                 | Read/Write off, optimize on, compress Medium |

Override only with a documented reason.

---

## 6. Profiling protocol

Run on every milestone build:

1. Connect device, build `profile` APK (IL2CPP, deep profiler off).
2. Unity Profiler → connect to device → record 60 s of normal play.
3. Capture Frame Debugger snapshot in Game scene.
4. Save Profiler `.data` and Frame Debugger summary in `Tools/profiles/<date>/`.
5. Compare against this budget. Regressions block the milestone.

Tools to standardize on:

- Unity Profiler
- Android GPU Inspector (AGI) for GPU-bound investigations
- `adb shell dumpsys gfxinfo <pkg>` for quick frame-time check

---

## 7. Optimization order (when over budget)

1. Reduce draw calls (batching, atlasing, mesh combine).
2. Reduce overdraw (water plane culling, transparent UI count).
3. Reduce texture memory (lower max size, ASTC block size).
4. Reduce shader cost (shader graph variants → keyword pruning).
5. Reduce script CPU (remove allocations, cache lookups).

Shaders and overdraw are the most common mobile bottlenecks - check them first.