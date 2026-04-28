# 📄 `docs/systems/audio-manager.md`

## Overview

Central audio router for music and SFX. Mentioned by `GameManager`; lives in
the Persistent scene.

---

## Responsibilities

- Play music tracks (one at a time, with crossfade)
- Play one-shot SFX
- Apply per-channel volume (music, SFX, master)
- Pause/resume audio with the game

---

## Ownership & Lifetime

- Lives in Persistent scene.
- Owned by GameManager.
- Single instance.

---

## Core data

- Master volume (0-1)
- Music volume (0-1)
- SFX volume (0-1)
- Current music track reference

Volumes persist via `PlayerPrefs` (small, non-gameplay state - exception to the
"no PlayerPrefs" rule).

---

## Public interface

- `PlayMusic(AudioClip clip, float fadeSeconds = 1f)`
- `StopMusic(float fadeSeconds = 1f)`
- `PlaySfx(AudioClip clip, float volumeScale = 1f)`
- `SetMusicVolume(float v)` / `SetSfxVolume(float v)` / `SetMasterVolume(float v)`

---

## Implementation notes

- Use a single `AudioMixer` with three groups: Master, Music, SFX.
- Volume sliders map to dB via `Mathf.Log10(v) * 20`, clamp `v ≥ 0.0001`.
- SFX uses a small pool of `AudioSource` (e.g. 8) to avoid `PlayOneShot` allocs.

---

## Interactions

### Uses

- Unity Audio (`AudioMixer`, `AudioSource`)
- `PlayerPrefs` for volume persistence

### Used by

- GameManager (pause → snapshot)
- UI (volume sliders)
- Gameplay (one-shot SFX requests)

---

## Constraints

- No streaming from network.
- No 3D spatialization in v0.2.
- No music in Boot scene.

---

## Failure handling

- Null clip → log warning, no-op.
- Mixer asset missing → fall back to direct AudioSource volume, log error.