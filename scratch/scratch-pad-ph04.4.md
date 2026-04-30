# Scratch Pad - artificial-pi

Useful commands accumulated during scaffolding. Will be coalesced into the
appropriate docs later.

---

## Phase 4

### Phase 4.4 - AudioManager

**Files created**

| File | Purpose |
|------|---------|
| `Assets/Scripts/Systems/Audio/IAudioManager.cs` | Public interface (PlayMusic, StopMusic, PlaySfx, Set*Volume) |
| `Assets/Scripts/Systems/Audio/AudioManager.cs` | MonoBehaviour implementation |
| `Assets/Tests/EditMode/AudioManagerTests.cs` | EditMode tests for VolumeToDb and ClampVolume |
| Matching `.meta` files | Unity asset tracking |

**AudioMixer setup required (HUMAN)**

Open the Unity Editor and:
1. Create `Assets/Settings/MainMixer.mixer` with three groups: `Master`, `Music`, `SFX`.
2. Expose the group attenuation parameters with **exact names**: `MasterVol`, `MusicVol`, `SfxVol`.
3. Assign the mixer asset and the three groups to `AudioManager`'s serialized fields in the Inspector.

**Design notes**

- SFX uses a pool of 8 `AudioSource` children to avoid `PlayOneShot` allocations.
- Music crossfade uses `IEnumerator` coroutines with `Time.unscaledDeltaTime` (works during pause).
- Volume dB mapping: `Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20f`; silence floor is -80 dB.
- Volumes persist via `PlayerPrefs` keys `audio.master`, `audio.music`, `audio.sfx`.
- If `_mixer` is null at Awake, logs an error and folds master/channel volumes into direct `AudioSource.volume` as fallback.
